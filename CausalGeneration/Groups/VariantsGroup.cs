using CausalGeneration.Edges;
using CausalGeneration.Nests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Groups
{
    public class VariantsGroup<TNodeValue> : NodesGroup<TNodeValue>
    {
        private CausalModel<TNodeValue> _model;

        public Guid AbstractNodeId { get; set; }
        private Guid? _actualImplementationNodeId;


        // private IEnumerable<CausalModelNode<TNodeValue>>? _nodes;
        private CausalModelNode<TNodeValue>[] GetNodes() {
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
                /*int edgesCount = node.CausesNest.Edges().Count();
                // У реализации несколько причин
                if (edgesCount > 1)
                    errors.Add(new CausalModelError("",
                        string.Format(uniquenessOfCause, node.Id, Id)));

                // Реализация ничего не реализует
                if (edgesCount == 0)
                    errors.Add(new CausalModelError("",
                        string.Format(noCause, node.Id, Id)));
                */
                
                if (node.CausesNest is ImplementationNest nest)
                {
                    ImplementationEdge oneEdge = nest.ImplementationEdge;

                    // Реализация не реализует абстрактную сущность
                    if (oneEdge.CauseId != AbstractNodeId)
                        errors.Add(new CausalModelError("",
                            string.Format("Вариант реализации (узел с id == {0})"
                        + " группы (id группы == {1}) не реализует свою абстрактную сущность, т.к. "
                        + "не связан с ней в модели.", node.Id, Id)));

                    if (oneEdge.Weight < double.Epsilon)
                    {
                        errors.Add(new CausalModelError("",
                            string.Format("Связь узла-реализации (id узла == {0})"
                            + " в группе (id группы == {2}) имеет нулевой вес.",
                            node.Id, Id)));
                    }
                } else
                {
                    errors.Add(new CausalModelError("",
                        "Гнездо причин представлено неверным типом"));
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
                ((ImplementationNest)node.CausesNest).ImplementationEdge.Weight);

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

                var oneEdge = ((ImplementationNest)(nodes[curNodeIndex].CausesNest)).ImplementationEdge;
                choice -= oneEdge.Weight;
            }
            _actualImplementationNodeId = nodes[curNodeIndex].Id;
        }

        public override bool ShouldBeDiscarded(CausalModelNode<TNodeValue> node)
            => node.Id != _actualImplementationNodeId;  // Остается только одна реализация
    }
}
