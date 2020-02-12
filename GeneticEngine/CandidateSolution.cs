namespace GeneticEngine
{
    public class CandidateSolution<T>
    {
        public float Fitness { get; set; } = 0;
        public T Solution { get; set; }

        override public string ToString()
        {
            return $"{Fitness} {Solution}";
        }
    }
}