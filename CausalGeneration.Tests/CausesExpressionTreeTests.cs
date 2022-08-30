using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CausalGeneration.Tests
{
    public class CausesExpressionTreeTests
    {
        [Fact]
        public void EdgeLeafTest()
        {
            var falseEdge = TestUtils.NewFalseEdge();
            var trueEdge = TestUtils.NewTrueEdge();

            Assert.False(new EdgeLeaf(falseEdge).Evaluate());
            Assert.True(new EdgeLeaf(trueEdge).Evaluate());
        }

        [Fact]
        public void ConjunctionOperationTest()
        {
            var falseEdge = TestUtils.NewFalseEdge();
            var falseEdge1 = TestUtils.NewFalseEdge();
            var trueEdge = TestUtils.NewTrueEdge();
            var trueEdge1 = TestUtils.NewTrueEdge();

            Assert.False(Expressions.And(falseEdge, falseEdge1).Evaluate());
            Assert.False(Expressions.And(falseEdge, trueEdge).Evaluate());
            Assert.False(Expressions.And(trueEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.And(trueEdge, trueEdge1).Evaluate());
        }

        [Fact]
        public void DisjunctionOperationTest()
        {
            var falseEdge = TestUtils.NewFalseEdge();
            var falseEdge1 = TestUtils.NewFalseEdge();
            var trueEdge = TestUtils.NewTrueEdge();
            var trueEdge1 = TestUtils.NewTrueEdge();


            Assert.False(Expressions.Or(falseEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.Or(falseEdge, trueEdge).Evaluate());
            Assert.True(Expressions.Or(trueEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.Or(trueEdge, trueEdge1).Evaluate());
        }

        [Fact]
        public void InversionTest()
        {
            var falseEdge = TestUtils.NewFalseEdge();
            var trueEdge = TestUtils.NewTrueEdge();

            Assert.True(Expressions.Not(falseEdge).Evaluate());
            Assert.False(Expressions.Not(trueEdge).Evaluate());
        }


        [Fact]
        public void EdgesTest()
        {
            const int TEST_SIZE = 5;

            var edges = new ProbabilityEdge[TEST_SIZE];
            for (int i = 0; i < TEST_SIZE; i++)
                edges[i] = TestUtils.NewTrueEdge();

            var or = Expressions.Or(edges);
            var and = Expressions.And(edges);
            var not = Expressions.Not(TestUtils.NewFalseEdge());

            Assert.Equal(TEST_SIZE, or.GetEdges().Count());
            Assert.Equal(TEST_SIZE, and.GetEdges().Count());
            Assert.Single(not.GetEdges());
            Assert.Single(Expressions.Edge(TestUtils.NewTrueEdge()).GetEdges());
        }
    }
}
