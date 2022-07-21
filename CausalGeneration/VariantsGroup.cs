using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class VariantsGroup<TNodeValue> : NodesGroup<TNodeValue>
    {
        private const string uniquenessOfCause = "Узел (id == {0})"
            + " группы (id группы == {1}) имеет более одной причинной связи.";
        private const string noCause = "Узел (id == {0})"
            + " группы (id группы == {1}) не имеет ни одной причинной связи.";
        private const string noAbstractNode = "Вариант реализации (узел с id == {0})"
            + " группы (id группы == {1}) не реализует свою абстрактную сущность, т.к. "
            + "не связан с ней в модели.";
        private const string wrongProbability = "Причинная связь узла (id узла == {0}, "
            + "id связи == {1})"
            + " группы (id группы == {2}) имеет значение {3}, больше, чем 1.";

        private CausalModel<TNodeValue> _model;

        public Guid AbstractNodeId { get; set; }
        private Guid? _implementationNodeId;


        // private IEnumerable<CausalModelNode<TNodeValue>>? _nodes;
        public CausalModelNode<TNodeValue>[] GetNodes() {
            // Узлы в каузальной модели могут изменяться при генерации,
            // поэтому узлы группы не кэшируются
            return _model.FindAllNodesOfGroup(Id).ToArray();
        }

        public VariantsGroup(Guid abstractNodeId, CausalModel<TNodeValue> model)
        {
            AbstractNodeId = abstractNodeId;
            _model = model;
        }

        public override ValidationResult ValidateNodes()
        {
            var nodes = GetNodes();
            List<CausalModelError> errors = new List<CausalModelError>();

            foreach (var node in nodes)
            {
                int edgesCount = node.CausesNest.Edges().Count();
                // У реализации несколько причин
                if (edgesCount > 1)
                    errors.Add(new CausalModelError("",
                        string.Format(uniquenessOfCause, node.Id, Id)));

                // Реализация ничего не реализует
                if (edgesCount == 0)
                    errors.Add(new CausalModelError("",
                        string.Format(noCause, node.Id, Id)));

                if (edgesCount == 1)
                {
                    var oneEdge = node.CausesNest.Edges().ElementAt(0);

                    // Реализация не реализует абстрактную сущность
                    if (oneEdge.CauseId != AbstractNodeId)
                        errors.Add(new CausalModelError("",
                            string.Format(noAbstractNode, node.Id, Id)));

                    // Реализация может не реализовывать АС из-за вероятности
                    // меньше 1
                    if (oneEdge.Probability < 1)
                    {
                        errors.Add(new CausalModelError("",
                            string.Format(wrongProbability, node.Id, Id)));
                    }
                }
            }
            return errors.Count == 0 ? ValidationResult.Success
                : ValidationResult.Failed(errors.ToArray());
        }

        public override void Define()
        {
            Random rnd = new Random();
            var nodes = GetNodes();

            // Сумма вероятностей для выбора единственной реализации
            double probSum = nodes.Sum(node =>
            {
                var oneEdge = node.CausesNest.SingleEdge();

                // Todo: Классы исключений
                if (oneEdge == null)
                    throw new NullReferenceException("У одного из узлов группы не оказалось причин");
                if (oneEdge.Probability == null)
                    throw new Exception("Вероятность одной из связей узлов группы оказалась "
                        + "неопределена. Генерация недостающих вероятностей не поддерживается.");
                return oneEdge.Probability.Value;
            });

            // Определение фактических вероятностей не требуется
            /*foreach (var node in nodes)
            {
                var oneEdge = node.CausesNest.SingleEdge();
                if (oneEdge.ActualProbability == null)
                {
                    // Значение double в промежутке от 1 до probSum
                    double actual = rnd.NextDouble(1, probSum);
                    oneEdge.ActualProbability = actual;
                }
            }*/

            // Определить Id единственной реализации
            // Алгоритм Roulette wheel selection
            double choice = rnd.NextDouble(0, probSum);
            int curNodeIndex = -1;
            while (choice >= 0)
            {
                curNodeIndex++;
                if (curNodeIndex >= nodes.Length)
                    curNodeIndex = 0;

                var oneEdge = nodes[curNodeIndex].CausesNest.SingleEdge();

                // Фактическая вероятность уже была определена в методе Define
#pragma warning disable CS8629 // Nullable value type may be null.
                choice -= oneEdge.Probability.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (curNodeIndex == -1)
            {
                // Если индекс не изменился, значит, сумма вероятностей == 0, чего не может быть
                throw new Exception("Сумма вероятностей реализаций равна нулю");
            }
            _implementationNodeId = nodes[curNodeIndex].Id;
        }

        public override bool ShouldBeDiscarded(CausalModelNode<TNodeValue> node)
            => node.Id != _implementationNodeId;  // Остается только одна реализация
    }
}
