using CausalGeneration.Edges;
using CausalGeneration.Groups;
using CausalGeneration.Nests;
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
        /// Корневые узлы требуются на этапе отбрасывания "мусорных" узлов
        /// </summary>
        private ISet<CausalModelNode<TNodeValue>> _roots;

        // Структура класса во многом обусловлена необходимостью представления в Json
        public List<CausalModelNode<TNodeValue>> Nodes { get; set; }

        public List<NodesGroup<TNodeValue>> Groups { get; set; }

        public CausalModelNode<TNodeValue>? FindNodeById(Guid id)
            => Nodes.FirstOrDefault(x => x.Id == id);

        public NodesGroup<TNodeValue>? FindGroupById(Guid id)
            => Groups.FirstOrDefault(x => x.Id == id);

        public List<CausalModelNode<TNodeValue>> FindAllNodesOfGroup(Guid groupId)
        {
            var nodes = Nodes.FindAll(node => node.GroupId == groupId);
            return nodes;
        }

        // Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            _roots = new HashSet<CausalModelNode<TNodeValue>>();
            Nodes = new List<CausalModelNode<TNodeValue>>();
            Groups = new List<NodesGroup<TNodeValue>>();
        }

        public CausalModelNode<TNodeValue> AddNode(CausalModelNode<TNodeValue> node)
        {
            Nodes.Add(node);
            return node;
        }
        public CausalModelNode<TNodeValue> AddNode(EdgesNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            var node = new CausalModelNode<TNodeValue>(causesNest, value);
            return AddNode(node);
        }

        public void AddVariantsGroup(CausalModelNode<TNodeValue> abstractNode,
            params TNodeValue[] values)
        {
            AddNode(abstractNode);
            VariantsGroup<TNodeValue> group = new VariantsGroup<TNodeValue>(abstractNode.Id, this);
            Groups.Add(group);
            foreach (var val in values)
            {
                var edge = new ImplementationEdge(1, abstractNode.Id);
                var nest = new ImplementationNest(edge);
                var node = new CausalModelNode<TNodeValue>(nest, val);
                node.GroupId = group.Id;
                AddNode(node);
            }
        }
        #endregion

        // Todo: Десериализация
        // Todo: Баг с сериализацией - CausesNest в CausalModel не сериализуется
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

        // Todo: Полная валидация
        #region Validation
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

        public ValidationResult Generate()
        {
            ValidationResult res = ValidateModel();
            if (!res.Succeeded)
                return res;

            DefineRoots();
            GenerationTrace();
            DiscardGarbageNodes();

            return ValidationResult.Success;
        }

        private void DefineRoots()
        {
            foreach (CausalModelNode<TNodeValue> node in Nodes)
            {
                if (node.EdgesNest.IsRootNest())
                    _roots.Add(node);
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
                }
                else if (node.EdgesNest is CausesNest nest)
                {
                    bool? isHappened = nest.IsHappened();
                    if (!isHappened.HasValue)
                        throw new Exception("Причинные связи узла не определены");
                    shouldBeDeleted = !isHappened.Value;
                }
                else
                {
                    throw new Exception("Узел имеет нестандартное гнездо связей, не находясь в группе");
                }

                if (shouldBeDeleted)
                {
                    DiscardNode(node);
                    continue;
                }

                // Для произошедших событий собираются следствия для включения в финальный
                // набор узлов
                foreach (Edge edge in node.EdgesNest.Edges)
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

        private void DefineNode(CausalModelNode<TNodeValue> node)
        {
            Random rnd = new Random();

            if (node.EdgesNest is CausesNest nest)
            {
                // Определить вероятности
                foreach (CausalEdge edge in nest.Edges)
                {
                    // Если не определена актуальная вероятность причинной связи
                    if (edge.ActualProbability == null)
                    {
                        edge.ActualProbability = rnd.NextDouble();
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
                    effect.EdgesNest.DiscardCause(node.Id);
                });
            }
            else
            {
                // Иначе это лист графа, он не имеет следствий
            }

            // 2. Для каждой причины узла удалять его из Effects, чтобы
            // в дальнейшем обходе по Effects узел не учитывался
            foreach (Edge edge in node.EdgesNest.Edges)
            {
                if (edge.CauseId == null)
                    continue;
                var cause = FindNodeById(edge.CauseId.Value);
                cause?.Effects?.Remove(node);
            }

            // 3. Удалить из _nodes, т.к. событие больше не нужно
            Nodes.Remove(node);

            // 4. Если данный узел - корневой, то также удалить из корней
            if (_roots.Contains(node))
            {
                _roots.Remove(node);
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
            foreach (CausalModelNode<TNodeValue> root in _roots)
            {
                AddNodeAndEffects(res, root);
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
