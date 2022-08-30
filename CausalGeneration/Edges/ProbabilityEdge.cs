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
        private double _probability;
        /// <summary>
        /// Вероятность того, что причинно-следственная связь повлечет за собой <br/>
        /// фактор события. Значение от 0 (фактор не происходит никогда) до 1.0
        /// (фактор происходит в любом случае)
        /// включительно <br />
        /// </summary>
        public double Probability
        {
            get => _probability;
            set
            {
                if (value >= 0 && value <= 1.0)
                {
                    _probability = value;
                }
                else
                    throw new ArgumentOutOfRangeException("",
                        "Некорректное значение вероятности");
            }
        }

        private double? _fixingValue;
        /// <summary>
        /// Значение, фиксирующее вероятность и определяющее, повлекла ли <br/>
        /// причинно-следственная связь за собой фактор события в текущей генерации. <br/>
        /// Значение больше или равно 0 (фактор происходит при любой ненулевой вероятности)
        /// и строго меньше 1.0 <br />
        /// В более старых описаниях может именоваться как Actual Probability.
        /// Может принимать значение null, например, до генерации
        /// </summary>
        public double? FixingValue
        {
            get => _fixingValue;
            set
            {
                if (value is null || value >= 0 && value < 1.0)
                {
                    _fixingValue = value;
                }
                else
                    throw new ArgumentOutOfRangeException("",
                        "Некорректное фиксирующее значение");
            }
        }

        /// <summary>
        /// true, если фиксирующее значение было сгенерировано. Данная пометка дает
        /// генерационной каузальной модели откатить сгенерированное значение
        /// </summary>
        internal bool IsGenerated { get; set; }

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
        /// значении. Значение вероятности превалирует над фиксирующим значением
        /// (если вероятность определена как 0 или 1, любое фиксирующее значение не может
        /// повлиять на исход фактора)
        /// </summary>
        /// <param name="probability">Вероятность, значение от 0 (связь не влечет за собой
        /// фактор в любом случае) до 1 (связь влечет фактор в любом случае)
        /// включительно</param>
        /// <param name="fixing">Фиксирующее значение - от 0 включительно (фактор
        /// происходит при любой ненулевой вероятности) до 1 невключительно</param>
        public static bool IsActuallyHappened(double probability, double fixing)
            => probability - fixing > 0;
    }
}
