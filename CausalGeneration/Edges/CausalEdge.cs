using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    public class CausalEdge : Edge
    {
        /// <summary>
        /// Вероятность того, что причинно-следственная связь повлечет за собой событие.
        /// <br/>
        /// Значение от 0 до 1.0 включительно <br />
        /// Может принимать значение null, например, если определяется на основе <br />
        /// сложных объектов, а не готовых данных, известных на момент создания
        /// </summary>
        virtual public double? Probability { get; set; }

        /// <summary>
        /// Значение, определяющее, повлекла ли причинно-следственная связь за собой
        /// событие в текущей генерации. <br/>
        /// Значение больше или равно 0 и строго меньше 1.0 <br />
        /// ActualProbability, строго говоря, - не вероятность <br />
        /// Может принимать значение null, например, до генерации
        /// </summary>
        virtual public double? ActualProbability { get; set; }

        public CausalEdge(double? probability = null, Guid? causeId = null,
            double? actualProbability = null) : base(causeId)
        {
            Probability = probability;
            ActualProbability = actualProbability;
        }

        public static double? GetTotalProbability(IEnumerable<CausalEdge> edges)
            => GetTotalProduct(edges, edge => edge.Probability);

        public static double? GetTotalActualProbability(IEnumerable<CausalEdge>
            edges) => GetTotalProduct(edges, edge => edge.ActualProbability);

        private static double? GetTotalProduct(IEnumerable<CausalEdge> edges,
            Func<CausalEdge, double?> getProperty)
        {
            double res = 1;
            foreach (CausalEdge edge in edges)
            {
                double? prop = getProperty(edge);
                if (!prop.HasValue)
                    return null;
                if (Math.Abs(prop.Value) < double.Epsilon)
                    return 0;
                res *= prop.Value;
            }
            return res;
        }

        public static bool? IsActuallyHappened(double? probability, double? actual)
            => actual == null || probability == null ?
            null : probability - actual > 0;

        public override string ToString()
        {
            string str = $"p = {Probability}; actual: {ActualProbability}; ";
            str += $"cause: {CauseId}";
            return str;
        }
    }
}
