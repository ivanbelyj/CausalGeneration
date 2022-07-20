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

            // Для десериализации json
            public CausesGroup() : this(new CausalModelEdge[] { }) { }

            public List<CausalModelEdge> Edges { get; set; }

            /// <summary>
            /// Todo: IsHappened свидетельствует только о выполнении необходимого условия
            /// происшествия события
            /// </summary>
            public bool? IsHappened()
            {
                double? actualTotalP = CausalModelEdge.GetTotalActualProbability(Edges);
                double? totalP = CausalModelEdge.GetTotalProbability(Edges);
                if (!actualTotalP.HasValue || !totalP.HasValue)
                    return null;
                return CausalModelEdge.IsActuallyHappened(totalP, actualTotalP);
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

        // Для десериализации json
        public CausesNest() : this(new CausesGroup[] { }) { }

        public CausesNest(Guid? causeId, double probability)
        {
            CausalModelEdge edge = new CausalModelEdge(probability, causeId);
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
