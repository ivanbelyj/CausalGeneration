using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    public class ImplementationEdge : Edge
    {
        /// <summary>
        /// Число, определяющее вероятность того, что абстрактный узел будет представлен <br/>
        /// данной реализацией. <br/>
        /// Значение строго больше 0, выбирается относительно весов других реализаций. <br />
        /// </summary>
        public double Weight { get; set; }

        public ImplementationEdge(double weight, Guid? causeId = null) : base(causeId)
        {
            Weight = weight;
        }
    }
}
