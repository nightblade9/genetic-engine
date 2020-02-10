using System;

namespace GeneticRoguelike.Model
{
    public abstract class Node
    {
        public Node[] Operands;

        public abstract float Evaluate();
    }

    public class OperatorNode : Node
    {
        private Func<float, float, float> operation;

        public OperatorNode(Func<float, float, float> operation, Node child1, Node child2)
        {
            this.operation = operation;
            this.Operands = new Node[] { child1, child2 };
        }

        override public float Evaluate()
        {
            return this.operation.Invoke(this.Operands[0].Evaluate(), this.Operands[1].Evaluate());
        }
    }

    public class VariableNode : Node
    {
        // Boxed int so we have a reference instead of a copy
        private VariableWrapper value;

        // There's only one variable: X
        public VariableNode(VariableWrapper value)
        {
            this.value = value;
        }

        override public float Evaluate()
        {
            return (int)this.value.Value;
        }
    }

    public class ConstantNode : Node
    {
        private int value;

        public ConstantNode(int constant)
        {
            this.value = constant;
        }

        override public  float Evaluate()
        {
            return this.value;
        }
    }

    public class VariableWrapper
    {
        public int Value { get; set; }

        public VariableWrapper(int value)
        {
            this.Value = value;
        }
    }
}