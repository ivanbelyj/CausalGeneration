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
    /// Представляет логическое выражение группировки причин.
    /// </summary>
    public abstract class CausesExpression
    {
        /// <summary>
        /// Получает конечное булевское значение минимальной единицы выражения -
        /// выполнено ли необходимое условие происшествия события (либо другого выражения)
        /// </summary>
        // public abstract bool EvaluateNecessary();

        /// <summary>
        /// true, если выполнено достаточное условие - произошли необходимые причинные события
        /// </summary>
        // public abstract bool EvaluateSufficient();
        // public virtual bool Evaluate() => EvaluateNecessary() && EvaluateSufficient();
        public abstract bool Evaluate();

        /// <summary>
        /// Ребра, включенные в логическое выражение. Требуется для генерации
        /// </summary>
        public abstract IEnumerable<ProbabilityEdge> GetEdges();

        /// <summary>
        /// Позволяет отбросить из выражения ребро с определенным id
        /// </summary>
        // public abstract void Discard(Guid edgeId);
    }
}
