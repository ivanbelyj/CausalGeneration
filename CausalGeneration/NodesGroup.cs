using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    abstract public class NodesGroup<TNodeValue>
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        /*public virtual NodesGroupValidationResult ApplyGroupRules<TNodeValue>(
            IEnumerable<CausalModelNode<TNodeValue>> nodes)
        {
            NodesGroupValidationResult res = ValidateNodes(nodes);
            if (!res.IsSuccessful)
                return res;
            ApplyGroupRules(nodes);
            return NodesGroupValidationResult.Successful;
        }*/

        public abstract ValidationResult ValidateNodes(
            IEnumerable<CausalModelNode<TNodeValue>> nodes);

        public abstract void ApplyGroupRules(
            IEnumerable<CausalModelNode<TNodeValue>> nodes);
    }
}
