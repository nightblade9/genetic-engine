using GeneticEngine;
using SampleSolutions.Model;

namespace SampleSolutions
{
    class Program
    {
        private const int WIDTH = 80;
        private const int HEIGHT = 29;
        
        static void Main(string[] args)
        {
            // POC: weight/value problem
            // new WeightValueSolver().EvolveSolution((generation, solution) =>
            // {
            //     System.Console.WriteLine($"Generation {generation}: fitness={solution.Fitness}");
            //     if (solution.Fitness < previousFitness)
            //     {
            //         throw new System.Exception($"GOTCHA!!! {previousFitness} => {solution.Fitness}!");
            //     }
            //     previousFitness = solution.Fitness;
            // });

            // Graph POC: curve-fitting problem
            new CurveFittingSolver().EvolveSolution((generation, solution) => 
            {
                System.Console.WriteLine($"Generation {generation}: fitness={solution.Fitness}");
            });
        }
    }
}
