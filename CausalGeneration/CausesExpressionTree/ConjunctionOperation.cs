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
        // public override bool EvaluateNecessary() => And(expr => expr.EvaluateNecessary());
        // public override bool EvaluateSufficient() => And(expr => expr.EvaluateSufficient());
        // public override bool Evaluate() => And(expr => expr.Evaluate());

        protected override bool Operation(bool[] operands)
        {
            foreach (bool operand in operands)
            {
                if (!operand)
                    return false;
            }
            return true;
        }
    }
}
