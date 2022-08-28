using CausalGeneration.Edges;
using CausalGeneration.CausesExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using CausalGeneration.Nests;
using CausalGeneration.Nodes;
using Newtonsoft.Json;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>
    {
        // Todo: можно добавить reference resolving, тогда можно будет хранить почти везде
        // непосредственно ссылки вместо id
        public HashSet<CausalModelNode<TNodeValue>> Nodes { get; set; }

        #region ModelCreation
        public CausalModel()
        {
            Nodes = new HashSet<CausalModelNode<TNodeValue>>();
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

        #region Json
        // Полиморфная сериализация/десериализация необходима для
        // CausesExpression => And, Or, Edge, Not
        // (Nest - нет)
        // (CausalEdge - нет)
        // CausalModelNode => ImplementationNode

        public string ToJson(bool writeIndented = false)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = writeIndented ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new KnownTypesSerializationBinder<TNodeValue>()
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static CausalModel<TNodeValue>? FromJson(string jsonString)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new KnownTypesSerializationBinder<TNodeValue>()
            };
            
            var model = JsonConvert.DeserializeObject<CausalModel<TNodeValue>>(jsonString,
                settings);
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
        private Dictionary<int, HashSet<CausalModelNode<TNodeValue>>>? _levelModel;

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

            _levelModel = new Dictionary<int, HashSet<CausalModelNode<TNodeValue>>>();
            // Для каждого узла определяется уровень
            foreach (var node in Nodes)
            {
                // Если элемент уже попал в соответствующий уровень
                if (node.HasLevel)
                    continue;

                int nodeLevel = DefineDepth(node);

                if (!_levelModel.ContainsKey(nodeLevel))
                {
                    _levelModel[nodeLevel] = new HashSet<CausalModelNode<TNodeValue>>();
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
        //private Dictionary<CausalModelNode<TNodeValue>, HashSet<ImplementationNode<TNodeValue>>>?
        //    _implementationGroups;

        /// <summary>
        /// На основе подготовленных данных обходит модель и генерирует результирующую
        /// </summary>
        private void LevelTrace()
        {
            if (_levelModel is null)
                throw new InvalidOperationException("Модель не подготовлена для обхода по уровням");

            var happened = new HashSet<CausalModelNode<TNodeValue>>();

            foreach ((var levelDepth, var level) in _levelModel)
            {
                // Определить узлы уровня для выполнения условия
                foreach (var node in level)
                {
                    DefineNode(node);
                }

                // Выбрать узлы, для которых выполнено необходимое условие (т.е., кроме условия
                // выбора единственной реализации)
                var necessary = new HashSet<CausalModelNode<TNodeValue>>();
                foreach (var node in level)
                {
                    // Гарантированно непроизошедшее
                    if (!node.ProbabilityNest.IsHappened())
                        continue;

                    // Необходимое условие выполнено, однако узел может оказаться
                    // невыбранной реализацией
                    necessary.Add(node);
                    // Если это не вариант реализации - условий для сущестования достаточно
                    if (!(node is ImplementationNode<TNodeValue>))
                    {
                        ((IHappenable)node).IsHappened = true;
                    }
                }

                // После отбрасывания того, что точно не произошло, можно получить
                // все группы вариантов и определить их одним разом.
                // Группа может простираться лишь по одному уровню, поэтому ее возможно получить
                var implGroups = GetLevelImplementationGroups(necessary);
                foreach ((var abstrEntity, var group) in implGroups)
                {
                    // Выбрать единственную реализацию из группы
                    var oneNode = SelectImplementation(group.ToArray());

                    // Узел остался без реализации, это нормально
                    if (oneNode is null)
                        continue;
                    ((IHappenable)oneNode).IsHappened = true;
                    // Остальные варианты остаются непомеченными
                }

                // Окончательная выборка
                foreach (var node in necessary)
                    if (((IHappenable)node).IsHappened ?? false)
                        happened.Add(node);
            }
            
            Nodes = happened;
        }

        private Dictionary<CausalModelNode<TNodeValue>,
            HashSet<ImplementationNode<TNodeValue>>>
            GetLevelImplementationGroups(HashSet<CausalModelNode<TNodeValue>> level)
        {
            var res = new Dictionary<CausalModelNode<TNodeValue>,
            HashSet<ImplementationNode<TNodeValue>>>();
            foreach (var node in level)
            {
                // Реализации АС, удовл. усл., откладываются в словарь
                // для быстрого получения всей группы
                if (node is ImplementationNode<TNodeValue> implNode)
                {
                    CausalModelNode<TNodeValue> abstractEntity = Nodes.First(x =>
                        x.Id == implNode.AbstractNodeId);
                    if (res.ContainsKey(abstractEntity))
                    {
                        res[abstractEntity].Add(implNode);
                    }
                    else
                    {
                        res.Add(abstractEntity,
                            new HashSet<ImplementationNode<TNodeValue>>() { implNode });
                    }
                }
            }
            return res;
        }

        // Todo: варианты с нулевым весом не учитываются
        // Todo: что, если не осталось ни одного ребра?
        private ImplementationNode<TNodeValue>? SelectImplementation(
            ImplementationNode<TNodeValue>[] nodes)
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
                if (curNodeIndex >= nodes.Length)
                    curNodeIndex = 0;

                choice -= nodes[curNodeIndex].WeightNest.TotalWeight();
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
