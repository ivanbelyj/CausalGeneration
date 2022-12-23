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
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace CausalGeneration.Model
{
    /// <summary>
    /// Используется для моделирования сущности, представимой
    /// в виде схемы из событий или свойств, связанных причинно-следственными связями.
    /// </summary>
    /// <typeparam name="TNodeValue">Тип значения, содержащегося в узле</typeparam>
    public class CausalGenerationModel<TNodeValue> : ICausalModel<TNodeValue>
    {
        // Todo: можно добавить reference resolving, тогда можно будет хранить почти везде
        // непосредственно ссылки вместо id

        /// <summary>
        /// Все узлы каузальной модели
        /// </summary>
        /// Коллекция наблюдаемая по двум причинам
        /// 1) Когда изменяется состав коллекции, требуется сделать подготовку
        /// обновленных данных для генерации. Делать подготовку, даже если коллекция
        /// не менялась, не эффективно
        /// 2) Отследить изменение с помощью методов по типу Add, Remove не получится,
        /// т.к. для сериализации требуется поле или свойство, а не методы
        public ObservableCollection<CausalModelNode<TNodeValue>> Nodes { get; set; }

        public string DeserializationTest { get; set; } = "test";

        IEnumerable<CausalModelNode<TNodeValue>> ICausalModel<TNodeValue>.Nodes
            => Nodes;


        /// <summary>
        /// Разложение модели по уровням. Индекс - уровень, значение - узлы уровня.
        /// </summary>
        private Dictionary<int, HashSet<CausalModelNode<TNodeValue>>>? _levelModel;
        private bool _needToReset = false;
        private bool _needToPreparate = true;
        private bool _needToResetPreparation = false;

        public CausalGenerationModel()
        {
            // Nodes = new HashSet<CausalModelNode<TNodeValue>>();
            Nodes = new ObservableCollection<CausalModelNode<TNodeValue>>();
            Nodes.CollectionChanged += (sender, e) => _needToPreparate = true;
        }

        #region ModelCreation
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

        // Todo: Полная валидация модели
        #region Validation
        /// <summary>
        /// Проверяет, соответствует ли модель нижеперечисленным требованиям.
        /// <list type="number">
        ///     <item>требование</item>
        ///     <item>требование</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public ValidationResult ValidateModel()
        {

            return ValidationResult.Success;
        }

        #endregion

        #region Generation
        /// <summary>
        /// Фиксирует данную генерационную модель и возвращает каузальную модель
        /// конкретной генерируемой ситуации.
        /// </summary>
        /// <param name="resultModel">Результирующая модель</param>
        public ValidationResult Generate(out CausalResultModel<TNodeValue> resultModel)
        {
            // Проверить корректность
            //ValidationResult res = ValidateModel();
            //if (!res.Succeeded)
            //    return res;

            if (_needToReset)
            {
                ResetGeneration();
                _needToReset = false;
            }
            if (_needToPreparate)
            {
                if (_needToResetPreparation)
                    ResetPreparation();

                // Представить модель в вид, пригодный для генерации
                Preparate();
                _needToPreparate = false;
                _needToResetPreparation = true;
            }

            // Обойти модель по уровням и сгенерировать результирующую модель
            var resModel = LevelTrace();

            // После генерации всегда требуется сброс, однако сброс требуется только
            // в случаях повторных генераций
            _needToReset = true;

            resultModel = resModel;

            return ValidationResult.Success;
        }



        /// <summary>
        /// Сбрасывает элементы состояния генерационной модели и ее составных
        /// элементов (ребер, узлов, и т.д.) для возможности повторной генерации
        /// </summary>
        private void ResetGeneration()
        {
            foreach (var node in Nodes)
            {
                ((IHappenable)node).IsHappened = false;
                foreach (var edge in node.GetEdges())
                    if (edge is ProbabilityEdge probEdge &&
                        probEdge.IsGenerated)
                    {
                        probEdge.FixingValue = null;
                        probEdge.IsGenerated = false;
                    }
            }
        }

        /// <summary>
        /// Сбрасывает изменения, внесенные подготовительным этапом, которые могут
        /// помешать следующей подготовке
        /// </summary>
        private void ResetPreparation()
        {
            foreach (var node in Nodes)
            {
                node.HasLevel = false;
            }
            _levelModel = null;
        }

        /// <summary>
        /// Этап предварительной подготовки данных для любых последующих генераций.
        /// Устанавливает ссылки на причины (вместо id),
        /// располагает узлы модели по уровням, опираясь на глубину
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

                    // Todo: Если во внешнем коде изменится CauseId,
                    // _needToPreparate не будет установлен. Кроме того,
                    // вызывать Preparate только для обновления Cause не эффективно

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
        private CausalResultModel<TNodeValue> LevelTrace()
        {
            if (_levelModel is null)
                throw new InvalidOperationException("Модель не подготовлена для обхода по уровням");

            // Узлы, входящие в окончательную выборку произошедшего
            var happened = new HashSet<CausalModelNode<TNodeValue>>();
            var abstrAndImpls = new Dictionary<CausalModelNode<TNodeValue>,
                CausalModelNode<TNodeValue>?>();

            // Цикл проходит по уровням (их можно сравнить с хронологией событий)
            // и выбирает то, что произошло
            foreach ((var levelDepth, var level) in _levelModel)
            {
                // Определить узлы уровня для выполнения необходимого условия
                foreach (var node in level)
                {
                    DefineNode(node);
                }

                // Выбрать узлы, для которых выполнено необходимое условие (но не условие
                // выбора единственной реализации, которое будет определено позже)
                var necessary = new HashSet<CausalModelNode<TNodeValue>>();
                foreach (var node in level)
                {
                    // Гарантированно непроизошедшее
                    if (!node.ProbabilityNest.IsHappened())
                    {
                        ((IHappenable)node).IsHappened = false;
                        continue;
                    }

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
                // Группа может простираться лишь по одному уровню, поэтому ее
                // возможно получить
                var implGroups = GetLevelImplementationGroups(necessary);
                foreach ((var abstrEntity, var group) in implGroups)
                {
                    // Выбрать единственную реализацию из группы
                    var oneNode = SelectImplementation(group.ToArray());

                    // Добавить в окончательный список реализаций и их абстрактных
                    // узлов для результата
                    if (((IHappenable)abstrEntity).IsHappened)
                        abstrAndImpls.Add(abstrEntity, oneNode);

                    // Узел остался без реализации, это нормально
                    if (oneNode is null)
                    {
                        continue;
                    }

                    ((IHappenable)oneNode).IsHappened = true;
                    // Остальные варианты остаются непомеченными
                }

                // Окончательная выборка
                // Todo: В результирующую модель добавляются те же самые узлы.
                // Изменения в результирующей модели приведут к мутациям в
                // генерационной
                foreach (var node in necessary)
                    if (((IHappenable)node).IsHappened)
                        happened.Add(node);
            }

            // Nodes = happened;
            return new CausalResultModel<TNodeValue>(happened.ToList(), abstrAndImpls);
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

        private ImplementationNode<TNodeValue>? SelectImplementation(
            ImplementationNode<TNodeValue>[] nodes)
        {
            Random rnd = new Random();

            // Собрать информацию о узлах и их общих весах, собрать сумму весов,
            // а также отбросить узлы с нулевыми весами
            var nodesWeights = new List<(CausalModelNode<TNodeValue> node, double totalWeight)>();
            double weightsSum = 0;
            foreach (var node in nodes)
            {
                double totalWeight = node.WeightNest.TotalWeight();
                if (totalWeight >= double.Epsilon)
                {
                    nodesWeights.Add((node, totalWeight));
                    weightsSum += totalWeight;
                }
            }
            if (weightsSum < double.Epsilon)
                return null;

            // Сумма вероятностей для выбора единственной реализации
            // double weightsSum = nodes.Sum(node => node.WeightNest.TotalWeight());

            // Определить Id единственной реализации
            // Алгоритм Roulette wheel selection
            double choice = rnd.NextDouble(0, weightsSum);
            int curNodeIndex = -1;
            while (choice >= 0)
            {
                curNodeIndex++;
                if (curNodeIndex >= nodes.Length)
                    curNodeIndex = 0;

                // choice -= nodes[curNodeIndex].WeightNest.TotalWeight();
                choice -= nodesWeights[curNodeIndex].totalWeight;
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
                if (edge.FixingValue is null)
                {
                    edge.FixingValue = rnd.NextDouble();
                    edge.IsGenerated = true;
                    // _needToReset = true;
                }
            }
        }
        #endregion
    }
}
