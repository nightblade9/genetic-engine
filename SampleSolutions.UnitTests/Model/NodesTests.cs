using System;
using SampleSolutions.Model;
using NUnit.Framework;

namespace SampleSolutions.UnitTests
{
    [TestFixture]
    public class NodesTests
    {

        [TestCase(-9)]
        [TestCase(0)]
        [TestCase(131)]
        public void EvaluateConstantNodeReturnsConstantValue(int value)
        {
            var node = new ConstantNode<int>(value);
            Assert.That(node.Evaluate(1), Is.EqualTo(value));
        }

        [TestCase(-11)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(14)]
        [TestCase(171)]
        public void EvaluateReturnsChangingValueForX(int value)
        {
            var wrapper = new VariableWrapper<int>(value);
            var node = new VariableNode<int>(wrapper);
            Assert.That(node.Evaluate(value), Is.EqualTo(value));

            wrapper.Value += 7;
            Assert.That(node.Evaluate(value + 7), Is.EqualTo(value + 7));
        }

        [Test]
        public void OperatorNodeEvaluatesBottomUp()
        {
            // y = (2 + 3) * (1 - (7 + x))
            // x = 3, so we get: -45
            var x = new VariableWrapper<float>(3);
            Func<float, float, float> add = (a, b) => a + b;
            Func<float, float, float> subtract = (a, b) => a - b;
            Func<float, float, float> multiplyNode = (a, b) => a * b;

            // We don't set parent nodes because we don't care about those in this test
            var leftSubtree = new OperatorNode<float>("add", add, new ConstantNode<float>(2), new ConstantNode<float>(3));
            var rightLeaf = new OperatorNode<float>("add", add, new ConstantNode<float>(7), new VariableNode<float>(x));
            var rightSubtree = new OperatorNode<float>("subtract", subtract, new ConstantNode<float>(1), rightLeaf);
            var root = new OperatorNode<float>("multiply", multiplyNode, leftSubtree, rightSubtree);

            var actual = root.Evaluate(x.Value);

            var expected = (2 + 3) * (1 - (7 + x.Value));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CloneClonesVariableNode()
        {
            var expected = new VariableNode<int>(new VariableWrapper<int>(37));
            var actual = expected.Clone();
            Assert.That(actual.Evaluate(999), Is.EqualTo(expected.Evaluate(999)));
            Assert.That(actual.Parent, Is.EqualTo(expected.Parent));
        }

    }
}