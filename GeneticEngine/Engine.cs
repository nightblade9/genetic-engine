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
    }
}