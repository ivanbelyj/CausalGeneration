using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    // Представляет элемент логического выражения, который вычисляется на основе
    // причинного ребра
    public class EdgeLeaf : CausesExpression
    {
        public ProbabilityEdge Edge { get; set; }

        public override IEnumerable<ProbabilityEdge> Edges
            => new List<ProbabilityEdge>() { Edge };

        public EdgeLeaf(ProbabilityEdge edge)
        {
            Edge = edge;
        }
        public override bool Evaluate()
        {
            if (Edge.FixingValue is null)
                throw new InvalidOperationException("Фиксирующее значение не установлено");

            // Todo: Получить причину по Edge.CauseId и проверить, избрана ли она
            bool condA = true; 
            bool condB = ProbabilityEdge.IsActuallyHappened(Edge.Probability,
                Edge.FixingValue.Value);
            return condA && condB;
        }
    }
}
