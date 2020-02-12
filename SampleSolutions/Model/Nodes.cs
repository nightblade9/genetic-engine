using System;

namespace SampleSolutions.Model
{
    public abstract class Node<T>
    {
        public Node<T> Parent { get; set; } // So we can replace a node
        
        public Node<T>[] Operands = null;

        public Node<T> Left {
            get { return this.Operands[0]; }
            set { this.Operands[0] = value; }
        }

        public Node<T> Right {
            get { return this.Operands[1]; }
            set { this.Operands[1] = value; }
        }

        public abstract T Evaluate(T x);

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
            return new OperatorNode<T>(this.operationName, this.operation, this.Left.Clone() as Node<T>, this.Right.Clone() as Node<T>);
        }

        override public T Evaluate(T x)
        {
            return this.operation.Invoke(this.Left.Evaluate(x), this.Right.Evaluate(x));
        }

        override public string ToString()
        {
            return $"({this.Left.ToString()} {this.operationName} {this.Right.ToString()})";
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

        override public T Evaluate(T x)
        {
            return x;
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

        override public T Evaluate(T x)
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