using CausalGeneration.Edges;
using CausalGeneration.CausesExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using CausalGeneration.Nests;
using CausalGeneration.Nodes;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>
    {
        public List<CausalModelNode<TNodeValue>> Nodes { get; set; }

        // Todo: Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            Nodes = new List<CausalModelNode<TNodeValue>>();
        }

        public void AddNodes(params CausalModelNode<TNodeValue>[] nodes)
        {
            foreach (var node in nodes)
                Nodes.Add(node);
        }
        public CausalModelNode<TNodeValue> AddNode(ProbabilityNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            var node = new CausalModelNode<TNodeValue>(causesNest, value);
            Nodes.Add(node);
            return node;
        }
        

        public void AddVariantsGroup(CausalModelNode<TNodeValue> abstractNode,
            params TNodeValue[] implementations)
        {
            Nodes.Add(abstractNode);
            
            foreach (var val in implementations)
            {
                // WeightNest nest = new WeightNest(abstractNode.Id, 1);
                // var node = new ImplementationNode<TNodeValue>(abstractNode.Id, nest);
                var node = NodeUtils.CreateImplementation(abstractNode.Id, 1, val);
                Nodes.Add(node);
            }
        }

        #endregion

        // Todo: Сериализация / десериализация
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

        // Todo: Полная валидация модели
        #region Validation
        public ValidationResult ValidateModel()
        {
            return ValidationResult.Success;
        }

        #endregion

        #region Generation

        public ValidationResult Generate()
        {
            // Проверить корректность
            //ValidationResult res = ValidateModel();
            //if (!res.Succeeded)
            //    return res;

            // Представить модель в вид, пригодный для генерации
            Preparate();

            // Обойти модель по уровням и сгенерировать результирующую модель
            LevelTrace();

            return ValidationResult.Success;
        }

        /// <summary>
        /// Разложение модели по уровням. Индекс - уровень, значение - узлы уровня.
        /// </summary>
        private Dictionary<int, List<CausalModelNode<TNodeValue>>>? _levelModel;

        /// <summary>
        /// Этап предварительной подготовки данных для любых последующих генераций.
        /// </summary>
        private void Preparate()
        {
            foreach (var node in Nodes)
            {
                // Иначе узел не корневой, а значит имеет причинные узлы.
                // Восстановить причины по id. id требуется для удобства представления
                // в json
                foreach (var edge in node.GetEdges())
                {
                    if (edge.CauseId is null)
                        continue;

                    // Причины берутся по id заранее для всех последующих генераций
                    // Todo: если элемент с таким id не существует - ошибка
                    edge.Cause = Nodes.First(x => x.Id == edge.CauseId);
                    if (edge.Cause is null)
                        throw new NullReferenceException("Узел с заданным CauseId не найден");
                }
            }

            _levelModel = new Dictionary<int, List<CausalModelNode<TNodeValue>>>();
            // Для каждого узла определяется уровень
            foreach (var node in Nodes)
            {
                // Если элемент уже попал в соответствующий уровень
                if (node.HasLevel)
                    continue;

                int nodeLevel = DefineDepth(node);

                if (!_levelModel.ContainsKey(nodeLevel))
                {
                    _levelModel[nodeLevel] = new List<CausalModelNode<TNodeValue>>();
                }
                _levelModel[nodeLevel].Add(node);
            }
        }

        //private int DefineLevel(CausalModelNode<TNodeValue> node)
        //{
        //    int maxCauseLevel = -1;
        //    // Если ни одно ребро не ссылается на узел модели
        //    if (!node.IsRootNode())
        //    {
        //        foreach (var edge in node.GetEdges())
        //        {
        //            // Некоторые ребра могут не иметь причин, это нормально
        //            if (edge.Cause is null)
        //                continue;

        //            var cause = ((CausalModelNode<TNodeValue>)(edge.Cause));
        //            DefineLevel(cause);
        //            if (cause.Level is null)
        //                throw new NullReferenceException("После процедуры определения "
        //                    + "глубин уровень причины оказался неопределенным.");
        //            int causeLevel = cause.Level.Value;

        //            if (causeLevel > maxCauseLevel)
        //                maxCauseLevel = causeLevel;
        //        }
        //    }

        //    node.Level = maxCauseLevel + 1;

        //    // Устанавливается глубина модели
        //    if (node.Level > _modelDepth)
        //        _modelDepth = node.Level;
        //}

        private int DefineDepth(CausalModelNode<TNodeValue> node)
        {
            int maxCauseLevel = -1;
            // Если ни одно ребро не ссылается на узел модели
            if (!node.IsRootNode())
            {
                foreach (var edge in node.GetEdges())
                {
                    // Некоторые ребра могут не иметь причин, это нормально
                    if (edge.Cause is null)
                        continue;

                    var cause = ((CausalModelNode<TNodeValue>)(edge.Cause));

                    int causeLevel = DefineDepth(cause);
                    if (causeLevel > maxCauseLevel)
                        maxCauseLevel = causeLevel;
                }
            }

            // Если это корневой узел - 0
            return maxCauseLevel + 1;
        }

        /// <summary>
        /// Абстрактные сущности (АС), найденные в модели, а также соответствующие
        /// совокупности их реализаций. В список включаются узлы, удовлетворяющие первому
        /// условию, это определяется в LevelTrace().
        /// </summary>
        private Dictionary<CausalModelNode<TNodeValue>, List<ImplementationNode<TNodeValue>>>?
            _implementationGroups;

        /// <summary>
        /// На основе подготовленных данных обходит модель и генерирует результирующую
        /// </summary>
        private void LevelTrace()
        {
            if (_levelModel is null)
                throw new InvalidOperationException("Модель не подготовлена для обхода по уровням");

            // Узлы, представляющие события, удовлетворяющие первому условию существования
            // в моделируемой ситуации
            var happened = new List<CausalModelNode<TNodeValue>>();

            _implementationGroups = new Dictionary<CausalModelNode<TNodeValue>,
                    List<ImplementationNode<TNodeValue>>>();

            foreach (var level in _levelModel)
            {
                // Определить узлы уровня
                foreach (var node in level.Value)
                {
                    DefineNode(node);
                }
                
                // Выбрать узлы, удовлетворяющие первому условию существования,
                // пока что пропуская варианты реализаций
                foreach (var node in level.Value)
                {
                    // Необходимое условие существования для любого узла
                    if (!node.ProbabilityNest.IsHappened())
                    {
                        ((IHappenable)node).IsHappened = false;
                        continue;
                    }

                    // Реализации АС, удовл. 1-му усл., откладываются в словарь
                    if (node is ImplementationNode<TNodeValue> implNode)
                    {
                        CausalModelNode<TNodeValue> abstractEntity = Nodes.First(x =>
                            x.Id == implNode.AbstractNodeId);
                        if (_implementationGroups.ContainsKey(abstractEntity))
                        {
                            _implementationGroups[abstractEntity].Add(implNode);
                        }
                        else
                        {
                            _implementationGroups.Add(abstractEntity,
                                new List<ImplementationNode<TNodeValue>>() { implNode });
                        }
                    }
                    else
                    {
                        ((IHappenable)node).IsHappened = true;
                        happened.Add(node);
                    }
                }
            }

            // Группы реализаций могут простираться по любым уровням,
            // однако требуется сделать выбор единственного варианта в момент, когда
            // первое условие существования уже определено для всех вариантов.
            foreach ((var abstrEntity, var group) in _implementationGroups)
            {
                // Выбрать единственную реализацию из группы
                var oneNode = SelectImplementation(group);

                // Узел остался без реализации, это нормально
                if (oneNode is null)
                    continue;

                happened.Add(oneNode);
            }
            
            Nodes = happened;
        }

        // Todo: варианты с нулевым весом не учитываются
        // Todo: что, если не осталось ни одного ребра?
        private ImplementationNode<TNodeValue>? SelectImplementation(
            List<ImplementationNode<TNodeValue>> nodes)
        {
            Random rnd = new Random();

            // Сумма вероятностей для выбора единственной реализации
            double probSum = nodes.Sum(node => node.WeightNest.TotalWeight());

            // Определить Id единственной реализации
            // Алгоритм Roulette wheel selection
            double choice = rnd.NextDouble(0, probSum);
            int curNodeIndex = -1;
            while (choice >= 0)
            {
                curNodeIndex++;
                if (curNodeIndex >= nodes.Count)
                    curNodeIndex = 0;

                choice -= nodes[curNodeIndex].WeightNest.TotalWeight();
            }

            // Для узлов группы отмечается, произошел ли каждый из них
            for (int i = 0; i < nodes.Count; i++)
            {
                ((IHappenable)nodes[curNodeIndex]).IsHappened = (i == curNodeIndex);
            }
            return nodes[curNodeIndex];
        }

        /// <summary>
        /// Определяет фиксирующие значения для всех причинных ребер узла
        /// </summary>
        private void DefineNode(CausalModelNode<TNodeValue> node)
        {
            Random rnd = new Random();

            // Определить неопределенные ранее фиксирующие значения для всех ребер
            foreach (ProbabilityEdge edge in node.ProbabilityNest.GetEdges())
            {
                if (edge.FixingValue == null)
                {
                    edge.FixingValue = rnd.NextDouble();
                }
            }
        }
        #endregion
    }
}
