using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using CausalGeneration.Nests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CausalGeneration.Tests
{
    public class CausalModelNodeTests
    {
        [Fact]
        public void IsRootNode()
        {
            CausalModel<string> model = new CausalModel<string>();

            // Root node
            var leaf1 = new EdgeLeaf(new ProbabilityEdge(1, null));
            var leaf2 = new EdgeLeaf(new ProbabilityEdge(1, null));
            var expression = new DisjunctionOperation(new[] { leaf1, leaf2 });
            var nest = new ProbabilityNest(expression);
            var rootNode = new CausalModelNode<string>(nest, "root node");
            model.Nodes.Add(rootNode);

            var leaf3 = new EdgeLeaf(new ProbabilityEdge(1, rootNode.Id));
            var leaf4 = new EdgeLeaf(new ProbabilityEdge(1, null));
            var expression1 = new ConjunctionOperation(new[] { leaf3, leaf4 });
            var nest1 = new ProbabilityNest(expression1);
            var notRootNode = new CausalModelNode<string>(nest1, "node name");
            model.Nodes.Add(notRootNode);

            Assert.True(rootNode.IsRootNode());
            Assert.False(notRootNode.IsRootNode());
        }

        [Fact]
        public void GetEdgesTest()
        {
            
        }
    }
}
