using System;

namespace GeneticRoguelike.Model
{
    public abstract class Node<T>
    {
        public Node<T> Parent { get; private set; } // So we can replace a node
        public Node<T>[] Operands;

        public abstract T Evaluate();
        public abstract Node<T> Clone();
    }

    public class OperatorNode<T> : Node<T>
    {
        private Func<T, T, T> operation;

        public OperatorNode(Func<T, T, T> operation, Node<T> child1, Node<T> child2)
        {
            this.operation = operation;
            this.Operands = new Node<T>[] { child1, child2 };
        }

        public override Node<T> Clone()
        {
            return new OperatorNode<T>(this.operation, this.Operands[0].Clone(), this.Operands[1].Clone());
        }

        override public T Evaluate()
        {
            return this.operation.Invoke(this.Operands[0].Evaluate(), this.Operands[1].Evaluate());
        }
    }

    public class VariableNode<T> : Node<T> 
    {
        // Boxed int so we have a reference instead of a copy
        private VariableWrapper<T> value;

        // There's only one variable: X
        public VariableNode(VariableWrapper<T> value)
        {
            this.value = value;
        }

        public override Node<T> Clone()
        {
            return new VariableNode<T>(this.value);
        }

        override public T Evaluate()
        {
            return this.value.Value;
        }
    }

    public class ConstantNode<T> : Node<T> 
    {
        private T value;

        public ConstantNode(T constant)
        {
            this.value = constant;
        }

        public override Node<T> Clone()
        {
            return new ConstantNode<T>(this.value);
        }

        override public T Evaluate()
        {
            return this.value;
        }
    }

    public class VariableWrapper<T>
    {
        public T Value { get; set; }

        public VariableWrapper(T value)
        {
            this.Value = value;
        }
    }
}