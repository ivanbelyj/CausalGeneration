using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using Xunit;

namespace CausalGeneration.Tests
{
    public class CausesExpressionTreeTests
    {
        private ProbabilityEdge NewFalseEdge() => new ProbabilityEdge(0, null, 0.5);
        private ProbabilityEdge NewTrueEdge() => new ProbabilityEdge(1, null, 0.5);

        [Fact]
        public void EdgeLeafTest()
        {
            var falseEdge = NewFalseEdge();
            var trueEdge = NewTrueEdge();

            Assert.False(new EdgeLeaf(falseEdge).Evaluate());
            Assert.True(new EdgeLeaf(trueEdge).Evaluate());
        }

        [Fact]
        public void ConjunctionOperationTest()
        {
            // Todo: что, если фиксирующее значение равно 0 при вероятности 0?
            var falseEdge = NewFalseEdge();
            var falseEdge1 = NewFalseEdge();
            var trueEdge = NewTrueEdge();
            var trueEdge1 = NewTrueEdge();

            Assert.False(Expressions.And(falseEdge, falseEdge1).Evaluate());
            Assert.False(Expressions.And(falseEdge, trueEdge).Evaluate());
            Assert.False(Expressions.And(trueEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.And(trueEdge, trueEdge1).Evaluate());
        }

        [Fact]
        public void DisjunctionOperationTest()
        {
            var falseEdge = NewFalseEdge();
            var falseEdge1 = NewFalseEdge();
            var trueEdge = NewTrueEdge();
            var trueEdge1 = NewTrueEdge();


            Assert.False(Expressions.Or(falseEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.Or(falseEdge, trueEdge).Evaluate());
            Assert.True(Expressions.Or(trueEdge, falseEdge1).Evaluate());
            Assert.True(Expressions.Or(trueEdge, trueEdge1).Evaluate());
        }

        [Fact]
        public void InversionTest()
        {
            var falseEdge = NewFalseEdge();
            var trueEdge = NewTrueEdge();

            Assert.True(Expressions.Not(falseEdge).Evaluate());
            Assert.False(Expressions.Not(trueEdge).Evaluate());
        }
    }
}
