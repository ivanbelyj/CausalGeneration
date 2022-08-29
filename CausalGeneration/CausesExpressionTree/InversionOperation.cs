using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    public class InversionOperation : CausesExpression
    {
        private CausesExpression _expression;
        public InversionOperation(CausesExpression expression)
        {
            _expression = expression;
        }

        public override IEnumerable<ProbabilityEdge> GetEdges() => _expression.GetEdges();

        //public override bool EvaluateNecessary()
        //{
        //    return !_expression.EvaluateNecessary();
        //}
        //public override bool EvaluateSufficient()
        //{
        //    return !_expression.EvaluateSufficient();
        //}

        public override bool Evaluate() => !_expression.Evaluate();
    }
}
