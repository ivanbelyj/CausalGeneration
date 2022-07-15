using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausesNest
    {
        public class CausesGroup
        {
            public CausesGroup(CausalModelEdge[] edges)
            {
                Edges = edges;
            }

            protected CausalModelEdge[] Edges { get; set; }

            public bool? IsHappened()
            {
                float? actualTotalP = GetTotalActualProbability();
                float totalP = GetTotalProbability();
                if (!actualTotalP.HasValue)
                    return null;
                return (totalP - actualTotalP.Value) >= 0;
            }

            public float GetTotalProbability() => GetTotalProduct(edge => edge.Probability);

            public float? GetTotalActualProbability()
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

            private float GetTotalProduct(Func<CausalModelEdge, float> getProperty)
            {
                float res = 1;
                foreach (CausalModelEdge edge in Edges)
                {
                    if (Math.Abs(getProperty(edge)) < float.Epsilon)
                        return 0;
                    res *= edge.Probability;
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

        public CausesNest(Guid? causeId, float probability)
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
        public bool? IsHappened
        {
            get
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
        }
    }
    
}
