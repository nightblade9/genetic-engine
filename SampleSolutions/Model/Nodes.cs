using System;

namespace SampleSolutions.Model
{
    public abstract class Node<T>
    {
        public Node<T> Parent { get; set; } // So we can replace a node
        public Node<T>[] Operands;

        public abstract T Evaluate();

        public abstract Node<T> Clone();
    }

    public class OperatorNode<T> : Node<T>
    {
        private Func<T, T, T> operation;
        private string operationName; // for debugging

        public OperatorNode(string operationName, Func<T, T, T> operation, Node<T> child1, Node<T> child2)
        {
            this.operationName = operationName;
            this.operation = operation;
            this.Operands = new Node<T>[] { child1, child2 };
            child1.Parent = this;
            child2.Parent = this;
        }

        override public Node<T> Clone()
        {
            return new OperatorNode<T>(this.operationName, this.operation, this.Operands[0].Clone() as Node<T>, this.Operands[1].Clone() as Node<T>);
        }

        override public T Evaluate()
        {
            return this.operation.Invoke(this.Operands[0].Evaluate(), this.Operands[1].Evaluate());
        }

        override public string ToString()
        {
            return $"({this.Operands[0].ToString()} {this.operationName} {this.Operands[1].ToString()})";
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

        override public Node<T> Clone()
        {
            return new VariableNode<T>(this.value);
        }

        override public T Evaluate()
        {
            return this.value.Value;
        }

        override public string ToString()
        {
            return "X";
        }
    }

    public class ConstantNode<T> : Node<T> 
    {
        private T value;

        public ConstantNode(T constant)
        {
            this.value = constant;
        }

        override public Node<T> Clone()
        {
            return new ConstantNode<T>(this.value);
        }

        override public T Evaluate()
        {
            return this.value;
        }

        override public string ToString()
        {
            return this.value.ToString();
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