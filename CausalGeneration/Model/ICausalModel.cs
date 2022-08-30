using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Model
{
    public interface ICausalModel<TNodeValue>
    {
        IEnumerable<CausalModelNode<TNodeValue>> Nodes { get; }
    }
}
