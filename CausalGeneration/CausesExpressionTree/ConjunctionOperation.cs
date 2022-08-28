using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    public class ConjunctionOperation : CausesOperation
    {
        public ConjunctionOperation(IEnumerable<CausesExpression> operands) : base(operands) { }
        //public override bool EvaluateNecessary()
        //{
        //    foreach (CausesExpression term in Operands)
        //    {
        //        if (!term.EvaluateNecessary())
        //            return false;
        //    }
        //    return true;
        //}
        public override bool EvaluateNecessary() => And(expr => expr.EvaluateNecessary());
        public override bool EvaluateSufficient() => And(expr => expr.EvaluateSufficient());

        private bool And(Predicate<CausesExpression> predicate)
        {
            foreach (CausesExpression operand in Operands)
            {
                if (!predicate(operand))
                    return false;
            }
            return true;
        }
    }
}
