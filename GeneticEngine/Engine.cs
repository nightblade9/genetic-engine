using System;
using System.Collections.Generic;

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
        
        // Given two parents, create one child
        private Func<T, T, T> oneChildCrossOverMethod = null;
        // Given two parents, create two children
        private Func<T, T, Tuple<T, T>> twoChildCrossOverMethod = null;

        private Func<T, T> mutationMethod = null;

        public Engine(int populationSize = 1000, float crossOverRate = 0.95f, float mutationRate = 0.1f)
        {
            this.populationSize = populationSize;
            this.crossOverRate = crossOverRate;
            this.mutationRate = mutationRate;
        }

        public T Solve()
        {
            return default(T);
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
    }
}