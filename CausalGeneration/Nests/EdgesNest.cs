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
        public abstract IEnumerable<Edge> Edges();
        public abstract void DiscardCause(Guid nodeId);
    }
    
}
