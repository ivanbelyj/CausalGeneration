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
    public class NestsTests
    {
        [Fact]
        public void IsRootNestTest()
        {
            // Root node
            var root = TestUtils.NewRootNest();
            var notRoot = TestUtils.NewNotRootNest();

            Assert.True(root.IsRootNest());
            Assert.False(notRoot.IsRootNest());
        }
    }
}
