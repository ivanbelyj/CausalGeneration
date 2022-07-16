using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausesNest : IEnumerable<CausalModelEdge>
    {
        public class CausesGroup
        {
            public CausesGroup(CausalModelEdge[] edges)
            {
                Edges = edges;
            }

            public CausalModelEdge[] Edges { get; set; }

            public bool? IsHappened()
            {
                double? actualTotalP = GetTotalActualProbability();
                double totalP = GetTotalProbability();
                if (!actualTotalP.HasValue)
                    return null;
                return (totalP - actualTotalP.Value) >= 0;
            }

            public double GetTotalProbability() => GetTotalProduct(edge => edge.Probability);

            public double? GetTotalActualProbability()
            {
                // Если хотя бы одна фактическая вероятность не определена
                if (Edges.Any(cause => !cause.ActualProbability.HasValue))
                {
                    return null;
                }
#pragma warning disable CS8629  // Nullable value type may be null.
                return GetTotalProduct(edge => edge.ActualProbability.Value);
#pragma warning restore CS8629  // Nullable value type may be null.
            }

            private double GetTotalProduct(Func<CausalModelEdge, double> getProperty)
            {
                double res = 1;
                foreach (CausalModelEdge edge in Edges)
                {
                    if (Math.Abs(getProperty(edge)) < double.Epsilon)
                        return 0;
                    res *= getProperty(edge);
                }
                return res;
            }
        }

        private CausesGroup[] _groups;

        public CausesNest(CausesGroup[] groups)
        {
            _groups = groups;
        }

        public CausesNest(params CausalModelEdge[] edges)
        {
            _groups = new CausesGroup[] { new CausesGroup(edges) };
        }

        public CausesNest(Guid? causeId, double probability)
        {
            CausalModelEdge edge = new CausalModelEdge()
            {
                Probability = probability,
                CauseId = causeId
            };
            CausalModelEdge[] edgesArr = new CausalModelEdge[] { edge };
            _groups = new CausesGroup[] { new CausesGroup(edgesArr) };
        }

        /// <summary>
        /// Определено на 2-м этапе генерации
        /// </summary>
        public bool? IsHappened()
        {
            foreach (CausesGroup item in _groups)
            {
                bool? isHappened = item.IsHappened();
                if (isHappened.HasValue)
                {
                    if (isHappened.Value)
                        return true;
                }
                else
                {
                    return null;
                }
            }
            return false;
        }

        public IEnumerator<CausalModelEdge> GetEnumerator()
        {
            foreach (CausesGroup group in _groups)
            {
                foreach (CausalModelEdge edge in group.Edges)
                {
                    yield return edge;
                }
            }
        }
        
        IEnumerator<CausalModelEdge> IEnumerable<CausalModelEdge>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}
