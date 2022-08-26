using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using CausalGeneration.Nests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public static class NodeUtils
    {
        public static CausalModelNode<TNodeValue> CreateNode<TNodeValue>(double probability,
            TNodeValue? value = default(TNodeValue), Guid? causeId = null)
            => new CausalModelNode<TNodeValue>(new ProbabilityNest(causeId, probability), value);

        private static CausalModelNode<TNodeValue> CreateNodeWithOperation<TNodeValue>(
            TNodeValue value, ProbabilityEdge[] edges,
            Func<ProbabilityEdge[], CausesOperation> operation)
            => new CausalModelNode<TNodeValue>(new ProbabilityNest(operation(edges)), value);

        /// <summary>
        /// Создает узел, имеющий множество причинных ребер. Ребра объединены
        /// логической операцией И
        /// </summary>
        public static CausalModelNode<TNodeValue> CreateNodeWithAnd<TNodeValue>(TNodeValue value,
            params ProbabilityEdge[] edges)
            => CreateNodeWithOperation(value, edges, Expressions.And);

        /// <summary>
        /// Создает узел, имеющий множество причинных ребер. Ребра объединены
        /// логической операцией ИЛИ
        /// </summary>
        public static CausalModelNode<TNodeValue> CreateNodeWithOr<TNodeValue>(TNodeValue value,
            params ProbabilityEdge[] edges)
            => CreateNodeWithOperation(value, edges, Expressions.Or);

        /// <summary>
        /// Создает узел-реализацию, связанный с абстрактным узлом единственным весовым ребром.
        /// Также узел имеет вероятностное ребро, обеспечивающее безусловную причину существования
        /// данного варианта реализации
        /// </summary>
        /// <returns></returns>
        public static ImplementationNode<TNodeValue> CreateImplementation<TNodeValue>(
            Guid abstractNodeId, double weight, TNodeValue? value = default(TNodeValue))
            => new ImplementationNode<TNodeValue>(abstractNodeId,
                new WeightNest(abstractNodeId, weight), new ProbabilityNest(null, 1), value);
    }
}
