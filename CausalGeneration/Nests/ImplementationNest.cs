﻿using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nests
{
    public class ImplementationNest : EdgesNest
    {
        /// <summary>
        /// Связывает реализацию с ее абстрактным узлом. null после отбрасывания
        /// </summary>
        public ImplementationEdge? ImplementationEdge { get; set; }
        public ImplementationNest(ImplementationEdge implementationEdge)
        {
            ImplementationEdge = implementationEdge;
        }

        public override IEnumerable<Edge> Edges
        {
            get
            {
                return ImplementationEdge == null ?
                    Array.Empty<Edge>() : new Edge[] { ImplementationEdge };
            }
            set
            {
                if (value.Count() > 1)
                    // Todo: ошибка, некорректное значение
                    throw new ArgumentException("Узел реализации не может иметь несколько причин");
                ImplementationEdge = (ImplementationEdge)value.First();
            }
        }

        public override void DiscardCause(Guid nodeId)
        {
            ImplementationEdge = null;
        }
    }
}
