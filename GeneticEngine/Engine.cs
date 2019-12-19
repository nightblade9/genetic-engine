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
        private float elitismPercent;
        private List<T> currentPopulation = new List<T>();
        private static Random random = new Random();
        
        // Given two parents, create one or more children
        private Func<T, T, List<T>> crossOverMethod = null;
        private Func<T, T> mutationMethod = null;
        private Func<T, float> calculateFitnessMethod = null;
        private Func<IList<CandidateSolution<List<T>>>, CandidateSolution<List<T>>> SelectionMethod = null;
        private Action<int, CandidateSolution<T>> onGenerationCallback = null;
        CandidateSolution<T> best = null;

        public static CandidateSolution<List<T>> TournamentSelection(IList<CandidateSolution<List<T>>> currentGeneration)
        {
            // Assumes a tournament of size=3
            var candidates = new List<CandidateSolution<List<T>>>();
            candidates.Add(RandomSelection(currentGeneration));
            candidates.Add(RandomSelection(currentGeneration));
            candidates.Add(RandomSelection(currentGeneration));
            
            var maxFitness = candidates.Max(c => c.Fitness);
            var winner = candidates.Single(c => c.Fitness == maxFitness);
            
            return winner;
        }

        public static CandidateSolution<List<T>> RandomSelection(IList<CandidateSolution<List<T>>> currentGeneration)
        {
            return currentGeneration[random.Next(currentGeneration.Count)];
        }

        public Engine(int populationSize, float elitismPercent, float crossOverRate, float mutationRate)
        {
            this.populationSize = populationSize;
            this.crossOverRate = crossOverRate;
            this.mutationRate = mutationRate;
            this.elitismPercent = elitismPercent;
        }

        public void Solve()
        {
            // TODO: validate population is created, cross-over/mutation methods are set, etc.

            var generation = 0;

            while (generation++ < 100) // Arbitrary. TODO: stop if fitness plateaus.
            {
                var fitnessScores = this.EvaulateFitness();
                best = fitnessScores.First();
                var average = fitnessScores.Average(f => f.Fitness);

                var nextGeneration = this.CreateNextGeneration(fitnessScores);
                this.currentPopulation = nextGeneration;
                
                if (this.onGenerationCallback != null)
                {
                    this.onGenerationCallback.Invoke(generation, best);
                }
            }
        }

        public void CreateInitialPopulation(Func<T> factoryMethod)
        {
            currentPopulation.Clear();
            while (currentPopulation.Count < this.populationSize)
            {
                currentPopulation.Add(factoryMethod.Invoke());
            }
        }

        public void SetCrossOverMethod(Func<T, T, List<T>> crossOverMethod)
        {
            this.crossOverMethod = crossOverMethod;
        }

        public void SetMutationMethod(Func<T, T> mutationMethod)
        {
            this.mutationMethod = mutationMethod;
        }

        public void SetFitnessMethod(Func<T, float> fitnessMethod)
        {
            this.calculateFitnessMethod = fitnessMethod;
        }

        public void SetSelectionMethod(Func<IList<CandidateSolution<List<T>>>, CandidateSolution<List<T>>> selectionMethod)
        {
            this.SelectionMethod = selectionMethod;
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
            var toReturn = new List<T>(this.populationSize);

            // currentGeneration is sorted best (fitness score) to worst, so handle elitism first
            var eliteCount = (int)(this.populationSize * this.elitismPercent);
            toReturn.AddRange(currentGeneration.Take(eliteCount).Select(s => s.Solution));

            while (toReturn.Count < this.populationSize)
            {
                List<T> result;
                var parent1 = currentGeneration[random.Next(currentGeneration.Count)];
                var parent2 = currentGeneration[random.Next(currentGeneration.Count)];

                if (random.NextDouble() <= this.crossOverRate)
                {
                    result = this.crossOverMethod.Invoke(parent1.Solution, parent2.Solution);
                }
                else
                {
                    result = new List<T>() { parent1.Solution, parent2.Solution };
                }

                foreach (var child in result)
                {
                    if (random.NextDouble() <= this.mutationRate)
                    {
                        this.mutationMethod(child);
                    }

                    if (random.NextDouble() <= this.mutationRate)
                    {
                        this.mutationMethod(child);
                    }
                }

                toReturn.AddRange(result);
            }

            return toReturn;
        }
    }
}