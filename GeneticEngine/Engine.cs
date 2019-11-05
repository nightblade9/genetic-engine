using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private Action<int, CandidateSolution<T>> onGenerationCallback = null;

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
                var nextGeneration = this.CreateNextGeneration(fitnessScores);
                this.currentPopulation = nextGeneration;
                
                if (this.onGenerationCallback != null)
                {
                    this.onGenerationCallback.Invoke(generation, best);
                }
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

        public void OnGenerationCallback(Action<int, CandidateSolution<T>> callback)
        {
            this.onGenerationCallback = callback;
        }

        // Makes a VERY big assumption that fitness is calculated independently per solution.
        // If this is wrong, prepare for the worst race conditions EVAR.
        private IList<CandidateSolution<T>> EvaulateFitness()
        {
            var toReturn = new List<CandidateSolution<T>>();

            Parallel.ForEach(this.currentPopulation, item =>
            {
                var score = this.calculateFitnessMethod(item);
                toReturn.Add(new CandidateSolution<T>() { Solution = item, Fitness = score });
            });

            return toReturn.OrderByDescending(t => t.Fitness).ToList();
        }

        private List<T> CreateNextGeneration(IList<CandidateSolution<T>> currentGeneration)
        {
            var toReturn = new List<T>();

            while (toReturn.Count < this.populationSize)
            {
                var parent1 = currentGeneration[random.Next(currentGeneration.Count)];
                var parent2 = currentGeneration[random.Next(currentGeneration.Count)];

                if (random.NextDouble() <= this.crossOverRate)
                {
                    var result = this.twoChildCrossOverMethod.Invoke(parent1.Solution, parent2.Solution);
                
                    if (random.NextDouble() <= this.mutationRate)
                    {
                        this.mutationMethod(result.Item1);
                    }

                    if (random.NextDouble() <= this.mutationRate)
                    {
                        this.mutationMethod(result.Item2);
                    }

                    toReturn.Add(result.Item1);
                    toReturn.Add(result.Item2);
                }
            }

            return toReturn;
        }
    }
}