using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModelEdge
    {
        /// <summary>
        /// Вероятность того, что причинно-следственная связь повлечет за собой событие.
        /// <br/>
        /// Значение от 0 до 1.0
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Значение, определяющее, повлекла ли причинно-следственная связь за собой
        /// событие в текущей генерации. <br/>
        /// До генерации - null. <br/>
        /// Значение от 0 до 1.0
        /// </summary>
        public double? ActualProbability { get; set; }

        /// <summary>
        /// Guid вершины, представляющей причину. <br/>
        /// null для корневых узлов
        /// </summary>
        public Guid? CauseId { get; set; }

        public override string ToString()
        {
            string str = $"p = {Probability}; actual: {ActualProbability}; ";
            str += $"cause: {CauseId}";
            return str;
        }
    }
}
