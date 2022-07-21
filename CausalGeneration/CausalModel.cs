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
    public class CausalModel<TNodeValue>
    {
        /// <summary>
        /// Id корневых узлов
        /// </summary>
        public ISet<Guid> Roots { get; set; } 
        // Todo: динамическое определение корней (узлы без причин перед генерацией)

        // Структура класса во многом обусловлена необходимостью представления в Json
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

            GenerationTrace();
            DiscardGarbageNodes();
            
            return ValidationResult.Success;
        }

        #region GroupsAndValidation
        public ValidationResult ValidateModel()
        {
            return ValidateGroups();
        }

        private ValidationResult ValidateGroups()
        {
            foreach (NodesGroup<TNodeValue> group in Groups)
            {
                ValidationResult res = group.ValidateNodes();
                if (!res.Succeeded)
                    return res;
            }
            return ValidationResult.Success;
        }
        
        #endregion

        #region Generation

        private void DefineNode(CausalModelNode<TNodeValue> node)
        {
            Random rnd = new Random();

            // Определить вероятности
            foreach (CausalModelEdge edge in node.CausesNest.Edges())
            {
                // Если не определена актуальная вероятность причинной связи
                if (edge.ActualProbability == null)
                {
                    edge.ActualProbability = rnd.NextDouble();
                }
            }
        }

        private void GenerationTrace()
        {
            // Определить узлы групп
            foreach (NodesGroup<TNodeValue> group in Groups)
            {
                group.Define();
            }

            foreach (CausalModelNode<TNodeValue> node in Nodes.ToList())
            {
                // Определить узел, чтобы в дальнейшем узнать, произошло ли событие
                if (node.GroupId == null)
                    DefineNode(node);

                // Отбрасывание непроизошедшего события
                bool shouldBeDeleted;

                // Некоторые группы переопределяют правила удаления
                if (node.GroupId != null)
                {
                    NodesGroup<TNodeValue>? group = FindGroupById(node.GroupId.Value);
                    if (group == null)
                        throw new Exception("Некорректный Id группы у узла");
                    shouldBeDeleted = group.ShouldBeDiscarded(node);
                } else
                {
                    bool? isHappened = node.CausesNest.IsHappened();
                    if (!isHappened.HasValue)
                        throw new Exception("Причинные связи узла не определены");
                    shouldBeDeleted = !isHappened.Value;
                }
                
                if (shouldBeDeleted)
                {
                    DiscardNode(node);
                    continue;
                }

                // Для произошедших событий собираются следствия для включения в финальный
                // набор узлов
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

        internal void DiscardNode(CausalModelNode<TNodeValue> node)
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
        #endregion
    }
}
