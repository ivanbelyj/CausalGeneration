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
    /// Узел, играющий роль варианта реализации абстрактной сущности (АС)
    /// в моделируемой ситуации.
    /// Абстрактная сущность - событие, свойство или факт, который может быть представлен
    /// в моделируемой ситуации в виде одного из вариантов (реализаций).
    /// Например, при генерации персонажа, его свойство "религия" может быть представлено
    /// одним из вариантов реализации АС.
    /// </summary>
    public class ImplementationNode<TNodeValue> : CausalModelNode<TNodeValue>
    {
        public WeightNest WeightNest { get; set; }
        public Guid AbstractNodeId { get; set; }
        public ImplementationNode(Guid id, Guid abstractNodeId, WeightNest weightNest,
            TNodeValue? value = default(TNodeValue), ProbabilityNest? probabilityNest = null)
            : base(id, probabilityNest ?? new ProbabilityNest(null, 1), value)
        {
            AbstractNodeId = abstractNodeId;
            WeightNest = weightNest;
        }

        public ImplementationNode(Guid abstractNodeId, WeightNest weightNest,
            ProbabilityNest probabilityNest, TNodeValue? value = default(TNodeValue))
            : this(Guid.NewGuid(), abstractNodeId, weightNest, value,
                  probabilityNest)
        { }

        //public ImplementationNode(Guid abstractNodeId, WeightNest weightNest,
        //    TNodeValue? value = default(TNodeValue))
        //    : this(Guid.NewGuid(), abstractNodeId, weightNest, value,
        //          new ProbabilityNest(null, 1))
        //{ }

        public override IEnumerable<CausalEdge> GetEdges()
            => WeightNest.GetEdges().Concat(
                (IEnumerable<CausalEdge>)ProbabilityNest.GetEdges());
        public override bool IsRootNode()
            => WeightNest.IsRootNest() && ProbabilityNest.IsRootNest();
    }
}
