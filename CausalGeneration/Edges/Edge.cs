using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    public class Edge
    {
        /// <summary>
        /// Guid вершины, представляющей причину. <br/>
        /// null для корневых узлов
        /// </summary>
        virtual public Guid? CauseId { get; }

        public Edge(Guid? causeId = null)
        {
            CauseId = causeId;
        }
    }
}
