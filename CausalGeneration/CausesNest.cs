using System;
using System.Collections;
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
                Edges = edges.ToList();
            }

            public List<CausalModelEdge> Edges { get; set; }

            /// <summary>
            /// Todo: IsHappened свидетельствует только о выполнении необходимого условия
            /// происшествия события.
            /// </summary>
            public bool? IsHappened()
            {
                double? actualTotalP = GetTotalActualProbability();
                double totalP = GetTotalProbability();
                if (!actualTotalP.HasValue)
                    return null;
                return (totalP - actualTotalP.Value) > 0;
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

        public CausesGroup[] Groups { get; set; }

        public CausesNest(CausesGroup[] groups)
        {
            Groups = groups;
        }

        public CausesNest(params CausalModelEdge[] edges)
        {
            Groups = new CausesGroup[] { new CausesGroup(edges) };
        }

        public CausesNest(Guid? causeId, double probability)
        {
            CausalModelEdge edge = new CausalModelEdge()
            {
                Probability = probability,
                CauseId = causeId
            };
            CausalModelEdge[] edgesArr = new CausalModelEdge[] { edge };
            Groups = new CausesGroup[] { new CausesGroup(edgesArr) };
        }

        public bool? IsHappened()
        {
            foreach (CausesGroup item in Groups)
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

        /// <summary>
        /// Используется на 2-м этапе генерации
        /// </summary>
        public void DiscardCause(Guid causeId)
        {
            foreach (CausesGroup group in Groups)
            {
                foreach (CausalModelEdge edge in group.Edges)
                {
                    if (edge.CauseId == causeId)
                    {
                        group.Edges.Remove(edge);
                        return;
                    }
                }
            }
        }

        public IEnumerable<CausalModelEdge> Edges()
        {
            foreach (CausesGroup group in Groups)
            {
                foreach (CausalModelEdge edge in group.Edges)
                {
                    yield return edge;
                }
            }
        }
    }
    
}
