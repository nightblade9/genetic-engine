namespace GeneticEngine
{
    /// <summary>
    /// THE genetic engine. Creates a population, evaluates fitness, cross-overs, mutates, repeats until fitness stabilizes.
    /// </summary>
    public class Engine
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
    }
}