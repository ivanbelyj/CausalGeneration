using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public struct CausalModelEdge
    {
        /// <summary>
        /// Вероятность того, что причинно-следственная связь повлечет за собой событие.
        /// <br/>
        /// Значение от 0 до 1.0
        /// </summary>
        public float Probability { get; set; }

        /// <summary>
        /// Значение, определяющее, повлекла ли причинно-следственная связь за собой
        /// событие в текущей генерации. <br/>
        /// До генерации - null. <br/>
        /// Значение от 0 до 1.0
        /// </summary>
        public float? ActualProbability { get; set; }

        /// <summary>
        /// Guid вершины, представляющей причину. <br/>
        /// null для корневых узлов
        /// </summary>
        public Guid? CauseId { get; set; }
    }
}
