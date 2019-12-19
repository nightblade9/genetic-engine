using System;
using System.Collections.Concurrent;
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
        private CandidateSolution<T> best = null;

        private IList<CandidateSolution<T>> DEBUG_PREVIOUS_GEN;
        private float DEBUG_PREVIOUS_FITNESS = 0;

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

                if (best.Fitness < DEBUG_PREVIOUS_FITNESS)
                {
                    //throw new InvalidOperationException($"GA broke: fitness dropped from {DEBUG_PREVIOUS_FITNESS} to {best.Fitness}!!!");
                }

                DEBUG_PREVIOUS_FITNESS = best.Fitness;
                DEBUG_PREVIOUS_GEN = fitnessScores.OrderByDescending(a => a.Fitness).ToList();
                var overlap = fitnessScores.Where(a => DEBUG_PREVIOUS_GEN.Any(b => b.Id == a.Id));
                Console.WriteLine($"{overlap.Count()} from previous gen made it to this gen");

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
            // List<T> is not thread-safe, so adding to it results in less elements
            // than the full population size.
            var evaluated = new ConcurrentBag<CandidateSolution<T>>();

            Parallel.ForEach(this.currentPopulation, item =>
            {
                var score = this.calculateFitnessMethod(item);
                evaluated.Add(new CandidateSolution<T>() { Solution = item, Fitness = score });
            });

            return evaluated.OrderByDescending(t => t.Fitness).ToList();
        }

        private List<T> CreateNextGeneration(IList<CandidateSolution<T>> currentGeneration)
        {
            var toReturn = new List<T>(this.populationSize);

            var eliteCount = (int)(this.populationSize * this.elitismPercent);
            
            var elites = currentGeneration.OrderByDescending(s => s.Fitness).Take(eliteCount);
            toReturn.AddRange(elites.Select(s => s.Solution));

            Console.WriteLine($"Elite {eliteCount} average fitness: {elites.Average(a => a.Fitness)}, top={elites.First().Fitness}");

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