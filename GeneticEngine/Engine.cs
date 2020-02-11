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
        
        // Not thread saff. Never used in multiple threads without a lock.
        private static Random random = new Random();
        private static object randomLock = new Object();
        
        // Given two parents, create one or more children
        private Func<T, T, List<T>> crossOverMethod = null;
        private Action<T> mutationMethod = null;
        private Func<T, float> calculateFitnessMethod = null;
        
        // Set of solutions in, and a single solution returned
        private Func<IList<CandidateSolution<T>>, CandidateSolution<T>> selectionMethod = null;
        private Action<int, CandidateSolution<T>> onGenerationCallback = null;
        private CandidateSolution<T> best = null;
        private List<float> previousGenerationScores = new List<float>();

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
            // Accept some small tolerable range before we say scores are stable
            float averageDifference = 999;
            float lastGenerationScore = 0;

            while (lastGenerationScore == 0 || averageDifference > 1)
            {
                generation++;
                
                // Calculate fitness
                var fitnessScores = this.EvaulateFitness();
                best = fitnessScores.First();

                // Horrible bug caused by parallel fitness evaluation giving non-deterministic results.

                // Sanity check: is elitism enabled? Is it big enough to be at least one thing?
                if (generation >= 2)
                {
                    float previousBest = previousGenerationScores[previousGenerationScores.Count - 1];
                    if (best.Fitness < previousBest && elitismPercent > 0 && (int)(elitismPercent * this.populationSize) >= 1)
                    {
                        throw new InvalidOperationException($"Elitism is enabled but fitness on generation {generation} dropped from {previousBest} to {best.Fitness}");
                    }
                }
                
                // Add a record of our best (keep only 10)
                previousGenerationScores.Add(best.Fitness);
                lastGenerationScore = best.Fitness;
                while (previousGenerationScores.Count > 10)
                {
                    previousGenerationScores.RemoveAt(0);
                }
                // Update what the average difference is
                if (previousGenerationScores.Count == 10)
                {
                    var average = previousGenerationScores.Average();
                    averageDifference = previousGenerationScores.Select(s => Math.Abs(s - average)).Sum();
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
            if (factoryMethod == null)
            {
                throw new ArgumentException(nameof(factoryMethod));
            }

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
            if (crossOverMethod == null)
            {
                throw new ArgumentException(nameof(crossOverMethod));
            }
            this.crossOverMethod = crossOverMethod;
        }

        /// <summary>
        /// Sets a mutation method; it takes a single T input, mutates it, and returns the mutated T as output.
        /// </summary>
        public void SetMutationMethod(Action<T> mutationMethod)
        {
            if (mutationMethod == null)
            {
                throw new ArgumentException(nameof(mutationMethod));
            }

            this.mutationMethod = mutationMethod;
        }

        /// <summary>
        /// Sets the fitness method, which returns a float value for the fitness of a candidate solution.
        /// </summary>
        public void SetFitnessMethod(Func<T, float> fitnessMethod)
        {
            if (fitnessMethod == null)
            {
                throw new ArgumentException(nameof(fitnessMethod));
            }

            this.calculateFitnessMethod = fitnessMethod;
        }

        /// <summary>
        /// Given a list of candidates, randomly selects three and return the fitest one.
        /// Public so you can call <c>engine.SetSelectionMethod(engine.TournamentSelection)</c>.
        /// </summary>
        public CandidateSolution<T> TournamentSelection(IList<CandidateSolution<T>> currentGeneration)
        {
            // Assumes a tournament of size=3
            if (currentGeneration == null || !currentGeneration.Any())
            {
                throw new ArgumentException("There are no candidate solutions to tournament-select from.");
            }

            var candidates = new List<CandidateSolution<T>>();
            while (candidates.Count < this.tournamentSize)
            {
                lock(randomLock) {
                    candidates.Add(currentGeneration[random.Next(currentGeneration.Count)]);
                }
            }
                        
            var maxFitness = candidates.Max(c => c.Fitness);
            // If we picked the same candidate multiple times, cool, just take any of them with max fitness
            var winner = candidates.First(c => c.Fitness == maxFitness);
            
            return winner;
        }

        /// <summary>
        /// Sets the selection method used when picking the fittest candidate solution from a set of solutions.
        /// The method takes a list of candidate solutions as input, and returns a single one as output.
        /// </summary>
        public void SetSelectionMethod(Func<IList<CandidateSolution<T>>, CandidateSolution<T>> selectionMethod)
        {
            if (selectionMethod == null)
            {
                throw new ArgumentException(nameof(selectionMethod));
            }

            this.selectionMethod = selectionMethod;
        }

        /// <summary>
        /// Set a callback that fires every time we finish evaluating a generation.
        /// The callback includes the generation number (eg. 3), and the best solution candidate of that generation.
        /// </summary>
        public void SetOnGenerationCallback(Action<int, CandidateSolution<T>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException(nameof(callback));
            }

            this.onGenerationCallback = callback;
        }

        // Makes a VERY big assumption that fitness is calculated independently per solution.
        // If this is wrong, prepare for the worst race conditions EVAR.
        private IList<CandidateSolution<T>> EvaulateFitness()
        {
            // List<T> is not thread-safe, so adding to it results in less elements
            // than the full population size.
            var evaluated = new ConcurrentBag<CandidateSolution<T>>();

            // I would LOVE to do this in parallel. The performance is better (around 10x).
            // BUT, there's a big problem. For some reason, doing this in parallel, with the
            // curve-fitting and roguelike samples, ends up with future generations having a
            // poorer fitness than previous generations. AND, for the SAME equation (eg. x^2),
            // I can see multiple instances in the generation, with different fitness scores!
            // Even though the fitness is non-random and completely deterministic!
            // So, SMH, just do this in serial for now... epic fail :<

            // NB: roguelike is a poor test, because it picks random points for fitness.
            
            //Parallel.ForEach(this.currentPopulation, item =>
            //{
            foreach (var item in this.currentPopulation)
            {
                var score = this.calculateFitnessMethod(item);
                evaluated.Add(new CandidateSolution<T>() { Solution = item, Fitness = score });
            }
            //});

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
                lock (randomLock)
                {
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
                }
                toReturn.AddRange(result);
            }

            return toReturn;
        }
    }
}