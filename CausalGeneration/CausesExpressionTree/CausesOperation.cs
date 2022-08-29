using CausalGeneration.Edges;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{

    /// <summary>
    /// Представляет операцию логической группировки причинных связей узлов.
    /// Операция может принимать множество аргументов
    /// </summary>
    public abstract class CausesOperation : CausesExpression
    {
        public CausesOperation(IEnumerable<CausesExpression> operands)
        {
            Operands = operands.ToList();
        }

        public IEnumerable<CausesExpression> Operands { get; set; }
        public override IEnumerable<ProbabilityEdge> GetEdges()
        {
            List<ProbabilityEdge> edges = new List<ProbabilityEdge>();
            foreach (CausesExpression operand in Operands)
            {
                edges.AddRange(operand.GetEdges());
            }
            return edges;
        }

        public override bool Evaluate() =>
            Operation(Operands.Select(expr => expr.Evaluate()).ToArray());

        protected abstract bool Operation(bool[] operands);
    }
}
