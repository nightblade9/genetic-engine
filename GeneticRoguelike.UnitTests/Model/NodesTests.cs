using System;
using GeneticRoguelike.Model;
using NUnit.Framework;

namespace GeneticRoguelike.UnitTests
{
    [TestFixture]
    public class NodesTests
    {

        [TestCase(-9)]
        [TestCase(0)]
        [TestCase(131)]
        public void EvaluateConstantNodeReturnsConstantValue(int value)
        {
            var node = new ConstantNode(value);
            Assert.That(node.Evaluate(), Is.EqualTo(value));
        }

        [TestCase(-11)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(14)]
        [TestCase(171)]
        public void EvaluateReturnsChangingValueForX(int value)
        {
            var wrapper = new VariableWrapper(value);
            var node = new VariableNode(wrapper);
            Assert.That(node.Evaluate(), Is.EqualTo(value));

            wrapper.Value += 7;
            Assert.That(node.Evaluate(), Is.EqualTo(value + 7));
        }

        [Test]
        public void OperatorNodeEvaluatesBottomUp()
        {
            // y = (2 + 3) * (1 - (7 + x))
            // x = 3, so we get: -45
            var x = new VariableWrapper(3);
            Func<float, float, float> add = (a, b) => a + b;
            Func<float, float, float> subtract = (a, b) => a - b;
            Func<float, float, float> multiplyNode = (a, b) => a * b;

            var leftSubtree = new OperatorNode(add, new ConstantNode(2), new ConstantNode(3));
            var rightLeaf = new OperatorNode(add, new ConstantNode(7), new VariableNode(x));
            var rightSubtree = new OperatorNode(subtract, new ConstantNode(1), rightLeaf);
            var root = new OperatorNode(multiplyNode, leftSubtree, rightSubtree);

            var actual = root.Evaluate();

            var expected = (2 + 3) * (1 - (7 + x.Value));
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}