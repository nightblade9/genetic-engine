using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticEngine;
using SampleSolutions.Model;

namespace SampleSolutions
{
    public class CurveFittingSolver
    {
        private const float PROBABILITY_OF_VARIABLE_NODE = 0.2f;
        // When generating trees, big trees are cool. Ditto for mutation, where we may be replacing a huge subtee with a new one.
        private const float PROBABILITY_OF_SUBTREE = 0.5f;
        private object randomLock = new Object();
        private Random random = new Random();

        private IDictionary<string, Func<float, float, float>> operations = new Dictionary<string, Func<float, float, float>>
        {
            { "+", new Func<float, float, float>((a, b) => a + b) },
            { "-", new Func<float, float, float>((a, b) => a - b) },
            { "×", new Func<float, float, float>((a, b) => a * b) },
            { "÷", new Func<float, float, float>((a, b) => b == 0 ? 1 : a / b) },
        };
        private IList<int> constants = new List<int>();
        private VariableWrapper<float> x = new VariableWrapper<float>(0);
        
        // x => correct/expected value of f(x)
        private IDictionary<float, float> data = new Dictionary<float, float>();

        public void EvolveSolution(Action<int, CandidateSolution<OperatorNode<float>>> callback)
        {
            for (var n = -5; n < 10; n++)
            {
                constants.Add(n);
            }

            this.LoadXAndExpectedValuesFromCsv();
            var tree = this.GenerateSubtree(PROBABILITY_OF_SUBTREE);
            var engine = new Engine<OperatorNode<float>, Object>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(this.CreateRandomTrees);
            engine.SetFitnessMethod(this.CalculateFitness);
            engine.SetCrossOverMethod(this.CrossOver);
            engine.SetSelectionMethod(engine.TournamentSelection);
            engine.SetMutationMethod(this.Mutate);
            engine.SetOnGenerationCallback(callback);
            engine.Solve();
        }

        // Pick a random node, pick a random op, and replace it with a new tree
        private void Mutate(OperatorNode<float> node)
        {
            if (node.Operands == null)
            {
                // We could mutate the node itself, but, meh.
                return;
            }

            var newParent = PickRandomNode(node);
            var newSubtree = this.GenerateSubtree(PROBABILITY_OF_SUBTREE);

            if (newParent.Operands != null)
            {
                lock (randomLock)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        newParent.Operands[0] = newSubtree;
                    }
                    else
                    {
                        newParent.Operands[1] = newSubtree;
                    }
                }
            }
            else
            {
                // Generate a tiny subtree
                newParent.Operands = new Node<float>[] { this.GenerateSubtree(0), this.GenerateSubtree(0) };
            }
        }

        private Node<float> PickRandomNode(Node<float> root)
        {
            // UGH, brain meltdown. Instead of recursion, just use regular ol' iteration.
            var nodesSeen = new List<Node<float>>();
            var toInvestigate = new List<Node<float>> { root };

            while (toInvestigate.Any())
            {
                var current = toInvestigate[0];
                toInvestigate.RemoveAt(0);

                // Don't ever add the root, we don't want to swap the whole tree.
                if (!nodesSeen.Contains(current) && current != root)
                {
                    nodesSeen.Add(current);
                }

                if (current.Operands != null)
                {
                    foreach (var child in current.Operands)
                    {
                        // Checks if seen later
                        toInvestigate.Add(child);
                    }
                }
            }
            
            lock (randomLock)
            {
                return nodesSeen.ElementAt(random.Next(nodesSeen.Count()));
            }
        }

        private List<OperatorNode<float>> CrossOver(OperatorNode<float> parent1, OperatorNode<float> parent2)
        {
            var child1 = parent1.Clone() as OperatorNode<float>;
            var child2 = parent2.Clone() as OperatorNode<float>;

            var node1 =  PickRandomNode(child1);
            var node2 =  PickRandomNode(child2);

            // Swap node1 and node2, ugh.
            // I am not sure if this code is correct.
            var oldNode1 = node1.Clone();
            if (node1.Parent.Operands[0] == node1) {
                node1.Parent.Operands[0] = node2;
            } else {
                node1.Parent.Operands[1] = node2;
            }

            if (node2.Parent.Operands[0] == node2) {
                node2.Parent.Operands[0] = oldNode1;
            } else {
                node2.Parent.Operands[1] = oldNode1;
            }

            // Parents last.
            var temp = node1.Parent;
            node1.Parent = node2.Parent;
            node2.Parent = temp;
            
            // Old code to swap random child of with random child of node2.
            /*
            bool replaceLeft;
            bool replaceWithLeft;

            lock (randomLock)
            {
                replaceLeft = random.NextDouble() < 0.5;
                replaceWithLeft = random.NextDouble() < 0.5;
            }

            // This code is greatly inelegant.
            var replace = replaceLeft ? node1.Operands[0] : node1.Operands[1];
            var replaceWith = replaceWithLeft ? node2.Operands[0] : node2.Operands[1];

            replaceWith.Parent = node1;
            replace.Parent = node2;

            if (replaceLeft)
            {
                node1.Operands[0] = replaceWith;
             } else {
                 node1.Operands[1] = replaceWith;
             }

            if (replaceWithLeft)
            {
                node2.Operands[0] = replace;
            } else {
                node2.Operands[1] = replace;
            }
            */

            return new List<OperatorNode<float>> { child1, child2 };
        }

        private void LoadXAndExpectedValuesFromCsv()
        {
            var rows = File.ReadAllLines(Path.Combine("..", "data.csv"));
            foreach (var row in rows)
            {
                var values = row.Split(',');
                float x = float.Parse(values[0]);
                float fX = float.Parse(values[1]);
                this.data[x] = fX;
            }
        }

        private float CalculateFitness(OperatorNode<float> root)
        {
            float total = 0;
            foreach (var x in this.data.Keys)
            {
                this.x.Value = x;
                float actualValue = root.Evaluate();
                float expectedValue = this.data[x];
                // Calculate how far off we are from the expected value. 
                total += Math.Abs(actualValue - expectedValue);
            }

            // Higher fitness is better, so negate
            return -total;
        }

        private OperatorNode<float> CreateRandomTrees()
        {
            // Ideally, this should be ramped half-and-half: half "full" trees, half "grow" trees.
            // But, my brain melted, so instead, we just generate random trees of totally random sizes.
            return GenerateSubtree(0.5f);
        }

        private OperatorNode<float> GenerateSubtree(float probabilityOfRecursing)
        {
            bool shouldRecurse;
            string operationName;

            lock (randomLock)
            {
                shouldRecurse = random.NextDouble() < probabilityOfRecursing;
                operationName = operations.Keys.ElementAt(random.Next(operations.Keys.Count));
            }

            if (shouldRecurse)
            {
                // Keep decaying probability so we don't get HUGE trees
                var left = GenerateSubtree(probabilityOfRecursing / 2);
                var right = GenerateSubtree(probabilityOfRecursing / 2);
                return new OperatorNode<float>(operationName, operations[operationName], left, right);
            }
            else
            {
                bool leftIsX;
                bool rightIsX;

                lock (randomLock)
                {
                    leftIsX = random.NextDouble() < PROBABILITY_OF_VARIABLE_NODE;
                    rightIsX = random.NextDouble() < PROBABILITY_OF_VARIABLE_NODE;
                }

                Node<float> left;
                Node<float> right;

                if (leftIsX)
                {
                    left = new VariableNode<float>(x);
                }
                else
                {
                    int value;
                    lock (randomLock)
                    {
                        value = constants[random.Next(constants.Count)];
                    }
                    left = new ConstantNode<float>(value);
                }

                if (rightIsX)
                {
                    right = new VariableNode<float>(x);
                }
                else
                {
                    int value;
                    lock (randomLock)
                    {
                        value = constants[random.Next(constants.Count)];
                    }
                    right = new ConstantNode<float>(value);
                }

                return new OperatorNode<float>(operationName, operations[operationName], left, right);
            }
        }
    }
}