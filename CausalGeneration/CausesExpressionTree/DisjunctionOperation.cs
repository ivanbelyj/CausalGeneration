using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    public class DisjunctionOperation : CausesOperation
    {
        public DisjunctionOperation(IEnumerable<CausesExpression> operands) : base(operands) { }
        //public override bool EvaluateNecessary()
        //{
        //    foreach (CausesExpression term in Operands)
        //    {
        //        if (term.EvaluateNecessary())
        //            return true;
        //    }
        //    return false;
        //}
        public override bool EvaluateNecessary() => Or(expr => expr.EvaluateNecessary());
        public override bool EvaluateSufficient() => Or(expr => expr.EvaluateSufficient());

        private bool Or(Predicate<CausesExpression> predicate)
        {
            foreach (CausesExpression operand in Operands)
            {
                if (predicate(operand))
                    return true;
            }
            return false;
        }
    }
}
