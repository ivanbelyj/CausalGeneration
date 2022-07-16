using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModelNode<TNodeValue>
    {
        public CausalModelNode(Guid id, CausesNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            CausesNest = causesNest;
            Id = id;
            Value = value;
        }
        public CausalModelNode(CausesNest causesNest,
            TNodeValue? value = default(TNodeValue))
                : this(Guid.NewGuid(), causesNest, value) { }

        public Guid Id { get; set; }

        public CausesNest CausesNest { get; set; }

        // public Guid GroupId { get; set; }

        /// <summary>
        /// Если null, то данное звено – только связующее
        /// </summary>
        public TNodeValue? Value { get; set; }

        /// <summary>
        /// Определяется после 1-го этапа генерации. Требуется для обхода
        /// графа на 2-ом этапе.
        /// </summary>
        internal CausalModelNode<TNodeValue>[]? Effects { get; set; } = null;

        public override string ToString()
        {
            string str = $"Node {Id}\n";
            str += Value?.ToString() + "\n";

            bool? isHappened = CausesNest.IsHappened();
            if (isHappened.HasValue)
                str += $"Is happened: {isHappened}\n";

            str += "Edges\n";
            foreach (CausalModelEdge edge in CausesNest)
            {
                str += $"\t{edge.ToString()}\n";
            }
            return str;
        }
    }
}
