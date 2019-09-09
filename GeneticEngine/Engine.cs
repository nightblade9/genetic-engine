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
        private Action<T> randomSolutionFactoryMethod;
        private List<T> currentPopulation = new List<T>();

        public Engine(int populationSize = 1000, float crossOverRate = 0.95, float mutationRate = 0.1)
        {
            this.populationSize = populationSize;
            this.crossOverRate = crossOverRate;
            this.mutationRate = mutationRate;
        }

        public T Solve()
        {
            return default(T);
        }

        public void CreateInitialPopulation(Action<T> factoryMethod)
        {
            currentPopulation.Clear();
            while (currentPopulation.Count < this.populationSize)
            {
                currentPopulation.Add(factoryMethod.Invoke());
            }
        }
    }
}