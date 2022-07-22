using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    public class CausesNest : EdgesNest
    {
        public class CausesGroup
        {
            public CausesGroup(CausalEdge[] edges)
            {
                Edges = edges.ToList();
            }

            // Для десериализации json
            public CausesGroup() : this(new CausalEdge[] { }) { }

            public List<CausalEdge> Edges { get; set; }

            public bool? IsHappened()
            {
                double? actualTotalP = CausalEdge.GetTotalActualProbability(Edges);
                double? totalP = CausalEdge.GetTotalProbability(Edges);
                if (!actualTotalP.HasValue || !totalP.HasValue)
                    return null;
                return CausalEdge.IsActuallyHappened(totalP, actualTotalP);
            }
        }

        public CausesGroup[] Groups { get; set; }

        public CausesNest(CausesGroup[] groups)
        {
            Groups = groups;
        }

        public CausesNest(params CausalEdge[] edges)
        {
            Groups = new CausesGroup[] { new CausesGroup(edges) };
        }

        // Для десериализации json
        public CausesNest() : this(new CausesGroup[] { }) { }

        public CausesNest(Guid? causeId, double probability)
        {
            CausalEdge edge = new CausalEdge(probability, causeId);
            CausalEdge[] edgesArr = new CausalEdge[] { edge };
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
        public override void DiscardCause(Guid causeId)
        {
            foreach (CausesGroup group in Groups)
            {
                foreach (CausalEdge edge in group.Edges)
                {
                    if (edge.CauseId == causeId)
                    {
                        group.Edges.Remove(edge);
                        return;
                    }
                }
            }
        }

        public override IEnumerable<Edge> Edges()
        {
            foreach (CausesGroup group in Groups)
            {
                foreach (CausalEdge edge in group.Edges)
                {
                    yield return edge;
                }
            }
        }

        /*public Edge SingleEdge()
        {
            if (Groups.Length != 1 || Groups[0].Edges.Count != 1)
                throw new InvalidOperationException("Гнездо причин имеет не единственную причину");
            return Groups[0].Edges[0];
        }*/
    }
}
