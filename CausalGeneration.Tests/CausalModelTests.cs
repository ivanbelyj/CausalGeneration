using CausalGeneration.Edges;
using CausalGeneration.Nests;
using CausalGeneration.CausesExpressionTree;
using System.Linq;
using Xunit;

namespace CausalGeneration.Tests
{
    public class CausalModelTests
    {
        [Fact]
        public void AddNodeTest()
        {
            // Arrange
            var model = new CausalModel<string>();
            for (int i = 0; i < 5; i++)
            {
                // Act
                model.AddNode(new ProbabilityNest(null, 1), "Root node " + (i + 1));
            }

            // Assert
            Assert.Equal(5, model.Nodes.Count);
        }

        [Fact]
        public void AddVariantsGroupTest()
        {
            // Arrange
            var model = new CausalModel<string>();
            var abstractNode = new CausalModelNode<string>(new ProbabilityNest(null, 1),
                "Race");
            var races = new string[] {"Cheaymea", "Meraymea", "Evoymea",
                "Myeuramea", "Eloanei" };

            // Act
            model.AddVariantsGroup(abstractNode, races);

            // Assert
            Assert.Equal(races.Length + 1, model.Nodes.Count);
            foreach (var node in model.Nodes) {
                if (node == abstractNode)
                    continue;
                var nodeEdges = node.GetEdges();
                var weightEdges = nodeEdges.OfType<WeightEdge>();
                var probabilityEdges = nodeEdges.OfType<ProbabilityEdge>();
                Assert.Single(weightEdges);
                Assert.Single(probabilityEdges);
            }
        }

        //[Fact]
        //public void LevelModelTest()
        //{
        //    // Arrange
        //    CausalModel<string> model = new CausalModel<string>();
        //    var hobbyRoot = new CausalModelNode<string>(new ProbabilityNest(null, 1),
        //        "Hobby");
        //    var drawingNest = new ProbabilityNest(hobbyRoot.Id, 1);
        //    var drawingNode = new CausalModelNode<string>(drawingNest);
        //    model.AddNodes(drawingNode, hobbyRoot);

        //    // Act
        //    model.Generate();

        //    // Assert
        //    Assert.NotNull(model.LevelModel);
        //    if (model.LevelModel is not null)
        //        Assert.Equal(2, model.LevelModel.Count);
        //}

        [Fact]
        public void TestTemplate()
        {

        }

    }
}
