using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>  // : IEnumerable<TNodeValue>
    {
        /// <summary>
        /// Id корневых узлов
        /// </summary>
        public ISet<Guid> Roots { get; set; }
        public List<CausalModelNode<TNodeValue>> Nodes { get; set; }

        public List<NodesGroup<TNodeValue>> Groups { get; set; }

        public CausalModelNode<TNodeValue>? FindNodeById(Guid id)
            => Nodes.FirstOrDefault(x => x.Id == id);

        public NodesGroup<TNodeValue>? FindGroupById(Guid id)
            => Groups.FirstOrDefault(x => x.Id == id);

        public List<CausalModelNode<TNodeValue>> FindAllNodesOfGroup(Guid groupId)
            => Nodes.FindAll(node => node.GroupId == groupId);

        // Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            Roots = new HashSet<Guid>();
            Nodes = new List<CausalModelNode<TNodeValue>>();
            Groups = new List<NodesGroup<TNodeValue>>();
        }

        public CausalModelNode<TNodeValue> AddNode(CausalModelNode<TNodeValue> node)
        {
            Nodes.Add(node);
            return node;
        }
        public CausalModelNode<TNodeValue> AddNode(CausesNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            var node = new CausalModelNode<TNodeValue>(causesNest, value);
            return AddNode(node);
        }

        public void AddRoot(Guid id)
        {
            Roots.Add(id);
        }

        public CausalModelNode<TNodeValue> AddRootNode(TNodeValue value, double probability)
        {
            var node = AddNode(new CausesNest(null, probability), value);
            AddRoot(node.Id);
            return node;
        }

        #endregion

        #region Json
        public async Task ToJsonAsync(Stream stream, bool writeIndented = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = writeIndented,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            await JsonSerializer.SerializeAsync(stream, this, options);
        }

        public string ToJson(bool writeIndented = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = writeIndented,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            return JsonSerializer.Serialize(this, options);
        }

        // Todo: Какой тип модели?
        public static CausalModel<TNodeValue>? FromJson(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            var model = JsonSerializer.Deserialize<CausalModel<TNodeValue>>(jsonString);
            return model;
        }
        #endregion

        public ValidationResult Generate()
        {
            ValidationResult res = ValidateModel();
            if (!res.Succeeded)
                return res;
            DiscardAllNotHappened();
            DiscardGarbageNodes();
            ApplyGroupsRules();
            return ValidationResult.Success;
        }

        private ValidationResult ValidateModel() => ValidateGroups();

        private ValidationResult ValidateGroups()
        {
            foreach (NodesGroup<TNodeValue> group in Groups)
            {
                ValidationResult res =
                    group.ValidateNodes(FindAllNodesOfGroup(group.Id));
                if (!res.Succeeded)
                    return res;
            }
            return ValidationResult.Success;
        }

        private void ApplyGroupsRules()
        {
            foreach (var group in Groups)
            {
                group.ApplyGroupRules(FindAllNodesOfGroup(group.Id));
            }
        }

        private void DiscardAllNotHappened()
        {
            Random rnd = new Random();
            foreach (CausalModelNode<TNodeValue> node in Nodes.Select(x => x).ToList())
            {
                foreach (CausalModelEdge edge in node.CausesNest.Edges())
                {
                    if (edge is CausalModelEdge probEdge
                        && !edge.ActualProbability.HasValue)
                    {
                        probEdge.ActualProbability = rnd.NextDouble();
                    }
                }
                // Можно вызывать IsHappened
                bool? isHappened = node.CausesNest.IsHappened();
                // Если такое вообще возможно
                if (!isHappened.HasValue)
                {
                    throw new Exception("Ошибка на этапе отбрасывания непроизошедшего." +
                        " Не определено, произошло ли событие.");
                }
                if (!isHappened.Value)
                {
                    DiscardNode(node);
                    continue;
                }

                // Для того, что, возможно, произошло, собираются следствия
                // для дальнейшего обхода.
                foreach (CausalModelEdge edge in node.CausesNest.Edges())
                {
                    // Если у узла есть причина, значит узел - ее следствие
                    if (edge.CauseId.HasValue)
                    {
                        CausalModelNode<TNodeValue>? cause =
                            FindNodeById(edge.CauseId.Value);
                        if (cause != null)
                        {
                            if (cause.Effects == null)
                            {
                                cause.Effects = new List<CausalModelNode<TNodeValue>>();
                            }
                            cause.Effects.Add(node);
                        }
                    }
                }
            }
        }

        private void DiscardNode(CausalModelNode<TNodeValue> node)
        {
            // 1. Для каждого следствия удалить node из гнезда причин,
            // чтобы не учитывать непроизошедшее событие в структуре
            // сгенерированной модели

            if (node.Effects != null)
            {
                node.Effects.ForEach(effect =>
                {
                    effect.CausesNest.DiscardCause(node.Id);
                });
            }
            else
            {
                // Иначе это лист графа, он не имеет следствий
            }

            // 2. Для каждой причины узла удалять его из Effects, чтобы
            // в дальнейшем обходе по Effects узел не учитывался
            foreach (CausalModelEdge edge in node.CausesNest.Edges())
            {
                if (edge.CauseId == null)
                    continue;
                var cause = FindNodeById(edge.CauseId.Value);
                cause?.Effects?.Remove(node);
            }

            // 3. Удалить из _nodes, т.к. событие больше не нужно
            Nodes.Remove(node);

            // 4. Если данный узел - корневой, то также удалить из корней
            if (Roots.Contains(node.Id))
            {
                Roots.Remove(node.Id);
            }
        }

        /// <summary>
        /// Путем прохода по модели от причин к следствиям, оставляет в модели только
        /// актуальные узлы
        /// </summary>
        private void DiscardGarbageNodes()
        {
            Nodes = ActualNodes();
        }

        private List<CausalModelNode<TNodeValue>> ActualNodes()
        {
            var res = new List<CausalModelNode<TNodeValue>>();
            foreach (Guid rootId in Roots)
            {
                AddNodeAndEffects(res, FindNodeById(rootId));
            }
            return res;
        }
        private void AddNodeAndEffects(List<CausalModelNode<TNodeValue>> list,
            CausalModelNode<TNodeValue>? node)
        {
            if (node != null)
            {
                if (node.Effects != null)
                {
                    foreach (CausalModelNode<TNodeValue> effect in node.Effects)
                    {
                        AddNodeAndEffects(list, effect);
                    }
                }
                list.Add(node);
            }
        }
    }
}
