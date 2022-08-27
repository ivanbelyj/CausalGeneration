using CausalGeneration.Edges;
using CausalGeneration.Nodes;
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
            var cause = (IHappenable?)Edge.Cause;

            if (cause is not null && cause.IsHappened is null)
                throw new NullReferenceException("Не определено, произошла ли причина");

            // Исход ребер, не имеющих причин, зависит лишь от них самих
            bool condA = cause is null ||
                (cause.IsHappened is not null && cause.IsHappened.Value);
            bool condB = ProbabilityEdge.IsActuallyHappened(Edge.Probability,
                Edge.FixingValue.Value);
            return condA && condB;
        }
    }
}
