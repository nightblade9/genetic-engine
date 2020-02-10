using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticEngine;
using GeneticRoguelike.Model;

namespace GeneticRoguelike
{
    public class CurveFittingSolver
    {
        private const float PROBABILITY_OF_VARIABLE_NODE = 0.2f; 
        private const float PROBABILITY_OF_SUBTREE = 0.5f;
        private object randomLock = new Object();
        private Random random = new Random();
        private IList<Func<float, float, float>> operators = new List<Func<float, float, float>>
        {
            new Func<float, float, float>((a, b) => a + b),
            new Func<float, float, float>((a, b) => a - b),
            new Func<float, float, float>((a, b) => a * b),
            new Func<float, float, float>((a, b) => b == 0 ? 0 : a / b),
        };
        private IList<int> constants = new List<int>();
        private VariableWrapper<float> x = new VariableWrapper<float>(0);
        
        // x => correct/expected value of f(x)
        private IDictionary<float, float> data = new Dictionary<float, float>();

        public void EvolveSolution(Action<int, CandidateSolution<OperatorNode<float>>> callback)
        {
            foreach (var n in Enumerable.Range(-5, 10))
            {
                constants.Add(n);
            }

            this.LoadXAndExpectedValuesFromCsv();
            var tree = this.GenerateSubtree(PROBABILITY_OF_SUBTREE);
            var engine = new Engine<OperatorNode<float>, Object>(1000, 0.1f, 0.95f, 0.05f);
            engine.CreateInitialPopulation(this.CreateRandomTrees);
            engine.SetFitnessMethod(this.CalculateFitness);
            engine.SetCrossOverMethod(this.CrossOver);
            engine.SetSelectionMethod(engine.TournamentSelection);
            engine.SetMutationMethod(this.Mutate);
            engine.SetOnGenerationCallback(callback);
            engine.Solve();
        }

        private OperatorNode<float> Mutate(OperatorNode<float> node)
        {
            var victim = PickRandomNode(node);
        }

        private Node<float> PickRandomNode(Node<float> root)
        {
            // UGH, brain meltdown. Instead of recursion, just use regular ol' iteration.
            var nodesSeen = new List<Node<float>>();
            var toInvestigate = new List<Node<float>> { root };

            while (toInvestigate.Any())
            {
                var current = toInvestigate[0];

                if (!nodesSeen.Contains(current))
                {
                    nodesSeen.Add(current);
                }

                if (current.Operands != null)
                {
                    foreach (var child in current.Operands)
                    {
                        // Assuming no cycles / have never seen this node
                        toInvestigate.Add(child);
                    }
                }
            }

            lock (randomLock)
            {
                return nodesSeen[random.Next(nodesSeen.Count)];
            }
        }

        private List<OperatorNode<float>> CrossOver(OperatorNode<float> parent1, OperatorNode<float> parent2)
        {
            var child1 = parent1.Clone();
            var child2 = parent2.Clone();

            var swap1 =  PickRandomNode(child1).Clone();
            var swap2 =  PickRandomNode(child2).Clone();
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
                // Are we losing precision by doing this?
                // Calculate how far off we are from the expected value
                total += Math.Abs(actualValue - expectedValue);
            }

            // Higher fitness is better, so negate?
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
            Func<float, float, float> operation;

            lock (randomLock)
            {
                shouldRecurse = random.NextDouble() < probabilityOfRecursing;
                operation = operators[random.Next(operators.Count)];
            }

            if (shouldRecurse)
            {
                // Keep decaying probability so we don't get HUGE trees
                var left = GenerateSubtree(probabilityOfRecursing / 2);
                var right = GenerateSubtree(probabilityOfRecursing / 2);
                return new OperatorNode<float>(operation, left, right);
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

                return new OperatorNode<float>(operation, left, right);
            }
        }
    }
}