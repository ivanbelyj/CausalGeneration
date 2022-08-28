using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    /// <summary>
    /// Предоставляет и структурирует совокупность причинных ребер, определяющую
    /// зависимость следствия от причины
    /// </summary>
    public class ProbabilityNest : Nest
    {
        /// <summary>
        /// Причинные ребра, структурированные в виде логического выражения. Значение
        /// выражения после вычисления определяет, произошло ли событие (узел модели)
        /// </summary>
        public CausesExpression CausesExpression { get; set; }

        // Для десериализации
        public ProbabilityNest() : this(null) { }
        public ProbabilityNest(CausesExpression expression)
        {
            CausesExpression = expression;
        }

        public ProbabilityNest(Guid? causeId, double probability)
        {

            ProbabilityEdge edge = new ProbabilityEdge(probability, causeId);
            CausesExpression = new EdgeLeaf(edge);
        }

        /// <summary>
        /// Определяет, имеет ли место событие или факт, представленный узлом каузальной
        /// модели, в конкретной генерируемой ситуации, на основе логического выражения,
        /// структурирующего причинные связи.
        /// Допустимо вызывать, только если известно, имеют ли место все причины события.
        /// </summary>
        public bool IsHappened() => CausesExpression.Evaluate();

        public bool IsHappenedNecessary() => CausesExpression.EvaluateNecessary();

        public override IEnumerable<ProbabilityEdge> GetEdges()
            => CausesExpression.GetEdges();

        //public override void DiscardEdge(Guid causeId)
        //{
        //    CausesExpression.Discard(causeId);
        //    /* foreach (CausesOperation group in OperationsRoot)
        //    {
        //        foreach (CausalEdge edge in group.Operands)
        //        {
        //            if (edge.CauseId == causeId)
        //            {
        //                group.Operands.Remove(edge);
        //                return;
        //            }
        //        }
        //    } */
        //}

        // public static ProbabilityNest HappenedRootNest => new ProbabilityNest(null, 1);
    }
}
