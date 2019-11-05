using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticEngine
{
    /// <summary>
    /// THE genetic engine. Creates a population, evaluates fitness, cross-overs, mutates, repeats until fitness stabilizes.
    /// Note that the type T is your solution candidate, and S is your state. (eg. T is list of ops, S is the dungeon state)
    /// </summary>
    public class Engine<T, S>
    {
        private int populationSize;
        private float crossOverRate;
        private float mutationRate;
        private List<T> currentPopulation = new List<T>();
        private Random random = new Random();
        
        // Given two parents, create one child
        // private Func<T, T, T> oneChildCrossOverMethod = null;
        // Given two parents, create two children
        private Func<T, T, Tuple<T, T>> twoChildCrossOverMethod = null;
        private Func<T, T> mutationMethod = null;
        private Func<T, float> calculateFitnessMethod = null;

        public Engine(int populationSize = 1000, float crossOverRate = 0.95f, float mutationRate = 0.1f)
        {
            this.populationSize = populationSize;
            this.crossOverRate = crossOverRate;
            this.mutationRate = mutationRate;
        }

        public T Solve()
        {
            // TODO: validate population is created, cross-over/mutation methods are set, etc.

            var generation = 0;
            CandidateSolution<T> best = null;

            while (generation++ < 100) // Arbitrary. TODO: stop if fitness plateaus.
            {
                var fitnessScores = this.EvaulateFitness();
                best = fitnessScores.First();
                Console.WriteLine($"Generation {generation}: best score is {best.Fitness}!");
                var nextGeneration = this.CreateNextGeneration(fitnessScores);
                this.currentPopulation = nextGeneration;
            }

            return best.Solution;
        }

        public void CreateInitialPopulation(Func<T> factoryMethod)
        {
            currentPopulation.Clear();
            while (currentPopulation.Count < this.populationSize)
            {
                currentPopulation.Add(factoryMethod.Invoke());
            }
        }

        // public void SetCrossOverMethod(Func<T, T, T> oneChildMethod)
        // {
        //     this.oneChildCrossOverMethod = oneChildMethod;
        // }

        public void SetCrossOverMethod(Func<T, T, Tuple<T, T>> twoChildMethod)
        {
            this.twoChildCrossOverMethod = twoChildMethod;
        }

        public void SetMutationMethod(Func<T, T> mutationMethod)
        {
            this.mutationMethod = mutationMethod;
        }

        public void SetFitnessMethod(Func<T, float> fitnessMethod)
        {
            this.calculateFitnessMethod = fitnessMethod;
        }

        private IEnumerable<CandidateSolution<T>> EvaulateFitness()
        {
            var toReturn = new List<CandidateSolution<T>>();

            foreach (var item in this.currentPopulation)
            {
                var score = this.calculateFitnessMethod(item);
                toReturn.Add(new CandidateSolution<T>() { Solution = item, Fitness = score });
            }

            return toReturn.OrderByDescending(t => t.Fitness);
        }

        private List<T> CreateNextGeneration(IEnumerable<CandidateSolution<T>> currentGeneration)
        {
            // TODOOOOOOOOOOOOOOOOO: we're missing tournament selection

            var toReturn = new List<T>();
            var currentGenerationIndex = 0;
            var currentGenerationCount = currentGeneration.Count();

            // Since items are ordered, just iterate until we get enough for a new generation.
            while (currentGenerationIndex < currentGenerationCount && toReturn.Count < this.populationSize)
            {
                var solution = currentGeneration.ElementAt(currentGenerationIndex);

                if (random.NextDouble() <= this.crossOverRate)
                {
                    var secondParent = currentGenerationIndex + 1 < currentGenerationCount ? currentGeneration.ElementAt(currentGenerationIndex + 1) : solution;
                    var result = this.twoChildCrossOverMethod.Invoke(solution.Solution, secondParent.Solution);
                    toReturn.Add(result.Item1);
                    toReturn.Add(result.Item2);
                }

                if (random.NextDouble() <= this.mutationRate)
                {
                    var result = this.mutationMethod(solution.Solution);
                    toReturn.Add(result);
                }

                currentGenerationIndex++;
            }

            if (toReturn.Count < this.populationSize)
            {
                // Not everything mutates, crosses over, etc. So pick the best if we're short-staffed
                var diff = this.populationSize - toReturn.Count;
                toReturn.AddRange(currentGeneration.Take(diff).Select(s => s.Solution));
            }

            return toReturn;
        }
    }
}