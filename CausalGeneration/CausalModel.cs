using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>  // : IEnumerable<TNodeValue>
    {
        /// <summary>
        /// Id корневых узлов
        /// </summary>
        private List<Guid> _roots;
        private List<CausalModelNode<TNodeValue>> _nodes;

        public CausalModelNode<TNodeValue>? FindNodeById(Guid id)
        {
            return _nodes.FirstOrDefault(x => x.Id == id);
        }

        // Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            _roots = new List<Guid>();
            _nodes = new List<CausalModelNode<TNodeValue>>();
        }

        public CausalModelNode<TNodeValue> AddNode(CausalModelNode<TNodeValue> node)
        {
            _nodes.Add(node);
            return node;
        }
        public CausalModelNode<TNodeValue> AddNode(CausesNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            var node = new CausalModelNode<TNodeValue>(causesNest, value);
            return AddNode(node);
        }

        public void AddRoot(Guid id)
        {
            _roots.Add(id);
        }

        public CausalModelNode<TNodeValue> AddRootNode(TNodeValue value, float probability)
        {
            var node = AddNode(new CausesNest(null, probability), value);
            AddRoot(node.Id);
            return node;
        }

        #endregion

        #region Json
        public CausalModel(string json) : this()
        {
            throw new NotImplementedException();
        }
        public string ToJson()
        {
            throw new NotImplementedException();
        }

        // Todo: Какой тип модели?
        public static CausalModel<TNodeValue> FromJson()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void BuildModel()
        {

        }
    }
}
