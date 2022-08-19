using CausalGeneration.Edges;
using CausalGeneration.Nests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    /// <summary>
    /// Представляет узел каузальной модели. Может представлять какую-либо сущность: <br />
    /// событие, существование абстракции, реализацию абстракции, и, теоретически, другие. <br />
    /// Связан с другими узлами с помощью гнезда связей, которое содержит ребра графа. <br />
    /// </summary>
    /// <typeparam name="TNodeValue">Тип значения, которое узел содержит</typeparam>
    public class CausalModelNode<TNodeValue>
    {
        public CausalModelNode(Guid id, EdgesNest edgesNest,
            TNodeValue? value = default(TNodeValue))
        {
            EdgesNest = edgesNest;
            Id = id;
            Value = value;
        }
        public CausalModelNode(EdgesNest causesNest,
            TNodeValue? value = default(TNodeValue))
                : this(Guid.NewGuid(), causesNest, value) { }

        public CausalModelNode() : this (null) { }
        
        public Guid Id { get; set; }

        public EdgesNest EdgesNest { get; set; }

        // public Guid GroupId { get; set; }

        /// <summary>
        /// Если null, то данное звено – только связующее
        /// </summary>
        public TNodeValue? Value { get; set; }

        /// <summary>
        /// Определяется после 1-го этапа генерации. Требуется для обхода
        /// графа на 2-ом этапе.
        /// </summary>
        internal List<CausalModelNode<TNodeValue>>? Effects { get; set; } = null;

        /// <summary>
        /// null, если узел не относится к группе
        /// </summary>
        public Guid? GroupId { get; set; }

        public override string ToString()
        {
            string str = $"Node {Id}\n";
            str += Value?.ToString() + "\n";

            if (EdgesNest is CausesNest causal)
            {
                bool? isHappened = causal.IsHappened();
                if (isHappened.HasValue)
                    str += $"Is happened: {isHappened}\n";
            }
            str += "Edges\n";
            foreach (Edge edge in EdgesNest.Edges)
            {
                str += $"\t{edge}\n";
            }
            return str;
        }
    }
}
