using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Model
{
    /// <summary>
    /// Результат каузальной генерации - каузальная модель фиксированной конкретной
    /// ситуации. Коллекция неизменяема
    /// </summary>
    /// <typeparam name="TNodeValue">Значение, которое содержат узлы</typeparam>
    public class CausalResultModel<TNodeValue> : ICausalModel<TNodeValue>
    {
        public ReadOnlyCollection<CausalModelNode<TNodeValue>> Nodes { get; }
        IEnumerable<CausalModelNode<TNodeValue>> ICausalModel<TNodeValue>.Nodes
            => Nodes;

        public Dictionary<CausalModelNode<TNodeValue>, CausalModelNode<TNodeValue>?>
            AbstractsAndImpls
        { get; }


        public CausalResultModel(IList<CausalModelNode<TNodeValue>> nodes,
            Dictionary<CausalModelNode<TNodeValue>, CausalModelNode<TNodeValue>?>
            abstractsAndImpls)
        {
            Nodes = new ReadOnlyCollection<CausalModelNode<TNodeValue>>(nodes);
            AbstractsAndImpls = abstractsAndImpls;
        }

        public bool IsAbstract(CausalModelNode<TNodeValue> node) =>
            AbstractsAndImpls.ContainsKey(node);

        public CausalModelNode<TNodeValue> GetAbstractByImplmentation(
            CausalModelNode<TNodeValue> implementation)
            => AbstractsAndImpls.First(pair => pair.Value == implementation).Key;
    }
}
