using System;
using System.Collections.Generic;
using GeneticEngine;
using System.Linq;

namespace SampleSolutions
{
    public class WeightValueSolver
    {
        private const int SOLUTION_LIST_SIZE = 1000;
        private const int MAX_WEIGHT = 2500;

        public class Valuable 
        {
            public int Weight = 0;
            public int Value = 0;
            public Valuable(int weight, int value)
            {
                this.Weight = weight;
                this.Value = value;
            }
        }

        private Random random = new Random();

        public void EvolveSolution(Action<int, CandidateSolution<List<Valuable>>> callback)
        {
            var engine = new Engine<List<Valuable>, Object>(1000, 0.1f, 0.95f, 0.05f);
            engine.CreateInitialPopulation(this.CreateRandomValuables);
            engine.SetFitnessMethod(this.CalculateFitness);
            engine.SetCrossOverMethod(this.CrossOver);
            engine.SetSelectionMethod(engine.TournamentSelection);
            engine.SetMutationMethod(this.Mutate);
            engine.SetOnGenerationCallback(callback);
            engine.Solve();
        }

        // Not part of the engine because it doesn't know if we want a tree, list, etc.
        private List<Valuable> CreateRandomValuables()
        {
            var random = new Random();
            var toReturn = new List<Valuable>();
            while (toReturn.Count < SOLUTION_LIST_SIZE)
            {
                var next = new Valuable(random.Next(100), random.Next(20));
                toReturn.Add(next);
            }
            return toReturn;
        }

        private void Mutate(List<Valuable> input)
        {
            var firstIndex = random.Next(input.Count);
            var secondIndex = random.Next(input.Count);
            // Swap. Don't care if they're the same
            var temp = input[firstIndex];
            input[firstIndex] = input[secondIndex];
            input[secondIndex] = temp;
        }

        private List<List<Valuable>> CrossOver(List<Valuable> parent1, List<Valuable> parent2)
        {
            var child1 = new List<Valuable>(parent1);
            var child2 = new List<Valuable>(parent2);
            var min = Math.Min(parent1.Count, parent2.Count);

            for (var i = 0; i < min; i++)
            {
                if (random.NextDouble() <= 0.5)
                {
                    var temp = child1[i];
                    child1[i] = child2[i];
                    child2[i] = temp;
                }
            }

            return new List<List<Valuable>>() { child1, child2 };
        }

        private float CalculateFitness(List<Valuable> solution)
        {
            this.Trim(solution);
            return solution.Sum(v => v.Value);
        }

        private void Trim(List<Valuable> solution)
        {
            return;
            
            // Repairs solutions that are overweight
            while (solution.Any() && solution.Sum(v => v.Weight) > MAX_WEIGHT)
            {
                // Drop things from the end until we're in business
                solution.RemoveAt(solution.Count - 1);
            }
        }
    }
}