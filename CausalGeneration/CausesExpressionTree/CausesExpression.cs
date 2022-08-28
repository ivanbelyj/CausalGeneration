using CausalGeneration.Edges;
using JsonSubTypes;
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
    //[JsonConverter(typeof(JsonSubtypes), "type")]
    //[JsonSubtypes.KnownSubType(typeof(ConjunctionOperation), "and")]
    //[JsonSubtypes.KnownSubType(typeof(DisjunctionOperation), "or")]
    //[JsonSubtypes.KnownSubType(typeof(EdgeLeaf), "edge")]
    //[JsonSubtypes.KnownSubType(typeof(InversionOperation), "not")]
    public abstract class CausesExpression
    {
        /// <summary>
        /// Получает конечное булевское значение минимальной единицы выражения -
        /// выполнено ли необходимое условие происшествия события (либо другого выражения)
        /// </summary>
        public abstract bool EvaluateNecessary();

        /// <summary>
        /// true, если выполнено достаточное условие - произошли необходимые причинные события
        /// </summary>
        public abstract bool EvaluateSufficient();
        public virtual bool Evaluate() => EvaluateNecessary() && EvaluateSufficient();
        

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
