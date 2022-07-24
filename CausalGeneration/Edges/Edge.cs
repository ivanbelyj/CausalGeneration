using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    public class Edge
    {
        protected Guid? _causeId;

        /// <summary>
        /// Guid вершины, представляющей причину. <br/>
        /// null для корневых узлов
        /// </summary>
        virtual public Guid? CauseId { get => _causeId; }

        public Edge(Guid? causeId = null)
        {
            _causeId = causeId;
        }
    }
}
