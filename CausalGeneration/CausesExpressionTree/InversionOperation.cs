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

        public override IEnumerable<ProbabilityEdge> Edges => _expression.Edges;

        public override bool Evaluate()
        {
            return !_expression.Evaluate();
        }
    }
}
