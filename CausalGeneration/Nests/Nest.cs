using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    public abstract class Nest
    {
        /// <summary>
        /// Определяет, является ли гнездо причин корневым. Все ребра, входящие <br />
        /// в такие гнезда, не ссылаются на узлы модели.
        /// </summary>
        public bool IsRootNest()
        {
            // GetEdges().All(edge => edge.CauseId == null);
            foreach (var edge in GetEdges())
            {
                if (edge.CauseId is not null)
                    return false;
            }
            return true;
        }

        public abstract IEnumerable<CausalEdge> GetEdges();
    }
}
