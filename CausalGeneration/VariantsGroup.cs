using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class VariantsGroup<TNodeValue> : NodesGroup<TNodeValue>
    {
        private const string uniquenessOfCause = "Узел (id == {0})"
            + " группы (id группы == {1}) имеет более одной причинной связи.";
        private const string noCause = "Узел (id == {0})"
            + " группы (id группы == {1}) не имеет ни одной причинной связи.";
        private const string noAbstractEntity = "Вариант реализации (узел с id == {0})"
            + " группы (id группы == {1}) не реализует свою абстрактную сущность, т.к. "
            + "не связан с ней в модели.";

        private CausalModel<TNodeValue> _model;

        public Guid AbstractEntityId { get; set; }

        // Todo: Заменить model в данном месте на что-то вроде NodesManager,
        // т.к. в этом классе требуется только получать узлы
        public VariantsGroup(Guid abstractEntityId, CausalModel<TNodeValue> model)
        {
            AbstractEntityId = abstractEntityId;
            _model = model;
        }

        public override void ApplyGroupRules(
            IEnumerable<CausalModelNode<TNodeValue>> nodes)
        {
            Console.WriteLine("Применяются правила группы " + Id);
            foreach (var node in nodes)
            {
                Console.WriteLine(node);
            }
        }

        public override ValidationResult ValidateNodes(
            IEnumerable<CausalModelNode<TNodeValue>> nodes)
        {
            List<CausalModelError> errors = new List<CausalModelError>();
            foreach (var node in nodes)
            {
                int edgesCount = node.CausesNest.Edges().Count();
                if (edgesCount > 1)
                    errors.Add(new CausalModelError("",
                        string.Format(uniquenessOfCause, node.Id, Id)));
                if (edgesCount == 0)
                    errors.Add(new CausalModelError("",
                        string.Format(noCause, node.Id, Id)));
                if (edgesCount == 1 && node.CausesNest.Edges().ElementAt(0).CauseId
                    != AbstractEntityId)
                    errors.Add(new CausalModelError("",
                        string.Format(noAbstractEntity, node.Id, Id)));
            }
            return errors.Count == 0 ? ValidationResult.Success
                : ValidationResult.Failed(errors.ToArray());
        }
    }
}
