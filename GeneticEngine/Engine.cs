using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticEngine
{
    /// <summary>
    /// THE genetic engine. Creates a population, evaluates fitness, cross-overs, mutates, repeats until fitness stabilizes.
    /// Note that the type T is your solution candidate, and S is your state. (eg. T is list of ops, S is the dungeon state).
    /// </summary>
    public class Engine<T, S>
    {
        private int tournamentSize;
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
        
        // Set of solutions in, and a single solution returned
        private Func<IList<CandidateSolution<T>>, CandidateSolution<T>> selectionMethod = null;
        private Action<int, CandidateSolution<T>> onGenerationCallback = null;
        private CandidateSolution<T> best = null;
        private List<float> lastTenGenerationScores = new List<float>();

        /// <summary>
        /// Given a list of candidates, randomly selects three and return the fittest one.
        /// </summary>
        internal CandidateSolution<T> TournamentSelection(IList<CandidateSolution<T>> currentGeneration)
        {
            // Assumes a tournament of size=3
            if (currentGeneration == null || !currentGeneration.Any())
            {
                throw new ArgumentException("There are no candidate solutions to tournament-select from.");
            }

            var candidates = new List<CandidateSolution<T>>();
            while (candidates.Count < this.tournamentSize)
            {
                candidates.Add(RandomSelection(currentGeneration));
            }
                        
            var maxFitness = candidates.Max(c => c.Fitness);
            var winner = candidates.Single(c => c.Fitness == maxFitness);
            
            return winner;
        }

        /// <summary>
        /// Given the current generation of candidates, return one at random.
        /// </summary>
        public static CandidateSolution<T> RandomSelection(IList<CandidateSolution<T>> currentGeneration)
        {
            return currentGeneration[random.Next(currentGeneration.Count)];
        }

        public Engine(int populationSize, float elitismPercent, float crossOverRate, float mutationRate, int tournamentSize = 3)
        {
            this.populationSize = populationSize;
            this.crossOverRate = crossOverRate;
            this.mutationRate = mutationRate;
            this.elitismPercent = elitismPercent;
            this.tournamentSize = tournamentSize;
        }

        public void Solve()
        {
            if (!this.currentPopulation.Any())
            {
                throw new InvalidOperationException("Please call CreateInitialPopulation before calling Solve");
            }
            else if (this.calculateFitnessMethod == null)
            {
                throw new InvalidOperationException("Please call SetFitnessMethod before calling Solve");
            }
            else if (this.crossOverMethod == null)
            {
                throw new InvalidOperationException("Please call SetCrossOverMethod before calling Solve");
            }
            else if (this.selectionMethod == null)
            {
                throw new InvalidOperationException("Please call SetSelectionMethod before calling Solve");
            }
            else if (this.mutationMethod == null)
            {
                throw new InvalidOperationException("Please call SetMutationMethod before calling Solve");
            }

            var generation = 0;
            float averageDifference = 999;
            float lastGenerationScore = 0;

            while (lastGenerationScore == 0 || averageDifference > 1)
            {
                generation++;
                
                // Calculate fitness
                var fitnessScores = this.EvaulateFitness();
                best = fitnessScores.First();

                // Add a record of our best (keep only 10)
                lastTenGenerationScores.Add(best.Fitness);
                lastGenerationScore = best.Fitness;
                while (lastTenGenerationScores.Count > 10)
                {
                    lastTenGenerationScores.RemoveAt(0);
                }
                // Update what the average difference is
                if (lastTenGenerationScores.Count == 10)
                {
                    var average = lastTenGenerationScores.Average();
                    averageDifference = lastTenGenerationScores.Select(s => Math.Abs(s - average)).Sum();
                }

                // Create the next generation
                var nextGeneration = this.CreateNextGeneration(fitnessScores);
                this.currentPopulation = nextGeneration;
                
                if (this.onGenerationCallback != null)
                {
                    this.onGenerationCallback.Invoke(generation, best);
                }                
            }

            Console.WriteLine($"Solved in {generation} generations, best fitness is {best.Fitness}; solution is {best.Solution}");
        }

        public void CreateInitialPopulation(Func<T> factoryMethod)
        {
            currentPopulation.Clear();
            while (currentPopulation.Count < this.populationSize)
            {
                currentPopulation.Add(factoryMethod.Invoke());
            }
        }

        /// <summary>
        /// Sets a cross-over method. This method takes two Ts as inputs, and returns one or more Ts (hence, List<T> output).
        /// </summary>
        public void SetCrossOverMethod(Func<T, T, List<T>> crossOverMethod)
        {
            this.crossOverMethod = crossOverMethod;
        }

        /// <summary>
        /// Sets a mutation method; it takes a single T input, mutates it, and returns the mutated T as output.
        /// </summary>
        public void SetMutationMethod(Func<T, T> mutationMethod)
        {
            this.mutationMethod = mutationMethod;
        }

        /// <summary>
        /// Sets the fitness method, which returns a float value for the fitness of a candidate solution.
        /// </summary>
        public void SetFitnessMethod(Func<T, float> fitnessMethod)
        {
            this.calculateFitnessMethod = fitnessMethod;
        }

        /// <summary>
        /// Sets the selection method used when picking the fittest candidate solution from a set of solutions.
        /// The method takes a list of candidate solutions as input, and returns a single one as output.
        /// </summary>
        public void SetSelectionMethod(Func<IList<CandidateSolution<T>>, CandidateSolution<T>> selectionMethod)
        {
            this.selectionMethod = selectionMethod;
        }

        /// <summary>
        /// Set a callback that fires every time we finish evaluating a generation.
        /// The callback includes the generation number (eg. 3), and the best solution candidate of that generation.
        /// </summary>
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

        // Creates the next generation, given the current generation. That is: applies elitism,
        // invokes cross-over and mutation, and presents the new generation.
        private List<T> CreateNextGeneration(IList<CandidateSolution<T>> currentGeneration)
        {
            var toReturn = new List<T>(this.populationSize);

            var eliteCount = (int)(this.populationSize * this.elitismPercent);
            
            var elites = currentGeneration.OrderByDescending(s => s.Fitness).Take(eliteCount);
            toReturn.AddRange(elites.Select(s => s.Solution));

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