using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    /// <summary>
    /// Представляет логическое выражение группировки причин.
    /// </summary>
    public abstract class CausesExpression
    {
        /// <summary>
        /// Получает конечное булевское значение минимальной единицы выражения -
        /// выполнены ли условия, необходимые для происшествия события
        /// (либо другого выражения)
        /// </summary>
        public abstract bool Evaluate();

        /// <summary>
        /// Ребра, включенные в логическое выражение
        /// </summary>
        public abstract IEnumerable<ProbabilityEdge> Edges { get; }

        /// <summary>
        /// Позволяет отбросить из выражения ребро с определенным id
        /// </summary>
        // public abstract void Discard(Guid edgeId);
    }
}
