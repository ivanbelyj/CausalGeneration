using CausalGeneration.Edges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    /// <summary>
    /// Представляет какие-либо связи узла без учета их ограничений и особенностей структуры <br />
    /// </summary>
    public abstract class EdgesNest
    {
        public abstract IEnumerable<Edge> Edges { get; }
        public abstract void DiscardCause(Guid nodeId);

        /// <summary>
        /// Определяет, является ли гнездо причин корневым. Все ребра, входящие в такие гнезда,<br />
        /// не ссылаются на узлы модели.
        /// </summary>
        /// <returns></returns>
        public bool IsRootNest()
            => Edges.All(edge => edge.CauseId == null);
    }
    
}
