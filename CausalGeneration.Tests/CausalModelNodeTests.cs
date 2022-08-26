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
            //var expression = Expressions.Or(new ProbabilityEdge(1, null),
            //    new ProbabilityEdge(1, null));
            //var nest = new ProbabilityNest(expression);
            //var rootNode = new CausalModelNode<string>(nest, "root node");
            //model.Nodes.Add(rootNode);

            //var expression1 = Expressions.Or(new ProbabilityEdge(1, rootNode.Id),
            //    new ProbabilityEdge(1, null));
            //var nest1 = new ProbabilityNest(expression1);
            //var notRootNode = new CausalModelNode<string>(nest1, "node name");
            //model.Nodes.Add(notRootNode);
            var rootNode = new CausalModelNode<string>(TestUtils.NewRootNest(), "root");
            var notRootNode = new CausalModelNode<string>(TestUtils.NewNotRootNest(), "not root");

            Assert.True(rootNode.IsRootNode());
            Assert.False(notRootNode.IsRootNode());
        }

        [Fact]
        public void GetEdgesTest()
        {
            const int TEST_SIZE = 5;

            var edges = new ProbabilityEdge[TEST_SIZE];
            for (int i = 0; i < TEST_SIZE; i++)
                edges[i] = TestUtils.NewTrueEdge();

            var or = Expressions.Or(edges);
            var and = Expressions.And(edges);
            var not = Expressions.Not(TestUtils.NewFalseEdge());

            var expr = Expressions.Or(or, and, not);

            CausalModelNode<string> node = new CausalModelNode<string>(new ProbabilityNest(expr),
                "root");
            Assert.Equal(TEST_SIZE * 2 + 1, node.GetEdges().Count());
        }
    }
}
