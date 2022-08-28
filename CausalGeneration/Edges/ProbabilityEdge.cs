using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    /// <summary>
    /// Представляет причинно-следственное ребро, которое указывает, с какой вероятностью
    /// один из факторов следствия происходит в случае происшествия причины.
    /// </summary>
    public class ProbabilityEdge : CausalEdge
    {
        /// <summary>
        /// Вероятность того, что причинно-следственная связь повлечет за собой <br/>
        /// фактор события. Значение от 0 до 1.0 включительно <br />
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Значение, фиксирующее вероятность и определяющее, повлекла ли <br/>
        /// причинно-следственная связь за собой фактор события в текущей генерации. <br/>
        /// Значение больше или равно 0 и строго меньше 1.0 <br />
        /// В более старом варианте может именоваться как Actual Probability
        /// Может принимать значение null, например, до генерации
        /// </summary>
        public double? FixingValue { get; set; }

        public ProbabilityEdge(double probability, Guid? causeId = null,
            double? actualProbability = null)
        {
            Probability = probability;
            CauseId = causeId;
            FixingValue = actualProbability;
        }

        public override string ToString()
        {
            string str = $"p = {Probability}; actual: {FixingValue}; ";
            str += $"cause: {CauseId}";
            return str;
        }

        /// <summary>
        /// Функция, определяющая, повлекла ли причинно-следственная связь за собой
        /// фактор события в текущей генерации при данных вероятности и фиксурующем
        /// значении
        /// </summary>
        /// <param name="probability">Вероятность</param>
        /// <param name="fixing">Фиксирующее значение</param>
        /// <returns></returns>
        public static bool IsActuallyHappened(double probability, double fixing)
            => probability - fixing > 0;
    }
}
