using CausalGeneration.Edges;
using CausalGeneration.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    public class WeightNest : Nest
    {
        private List<WeightEdge> _edges { get; } = new List<WeightEdge>();

        public WeightNest(params WeightEdge[] edges)
        {
            _edges = edges.ToList();
        }

        /// <summary>
        /// Создает весовое гнездо, которое имеет единственное весовое ребро
        /// </summary>
        public WeightNest(Guid? causeId, double weight = 1)
        {
            _edges.Add(new WeightEdge(weight, causeId));
        }

        /// <summary>
        /// Подсчитывает общий вес гнезда, основываясь на весовых ребрах, связанных
        /// с произошедшими причинными событиями
        /// </summary>
        /// <returns></returns>
        public double TotalWeight()
        {
            if (_edges.Count == 0)
                throw new InvalidOperationException("Весовое гнездо не имеет ребер");

            double weightSum = 0;

            // Todo: total weight
            foreach (var edge in _edges)
            {
                var cause = (IHappenable?)edge.Cause;

                // Если у весового гнезда нет причины, считается, что вес всегда влияет на выбор
                if (cause is null)
                {
                    weightSum += edge.Weight;
                    continue;
                }

                if (cause.IsHappened is null)
                    throw new NullReferenceException("На этапе вычисления суммы весового гнезда "
                        + " не было определено, произошло ли причинное событие");

                if (cause.IsHappened.Value)
                    weightSum += edge.Weight;
            }

            return weightSum;
        }

        public override IEnumerable<CausalEdge> GetEdges() => _edges;
    }
}
