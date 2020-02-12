using GeneticEngine;
using SampleSolutions.Model;

namespace SampleSolutions
{
    class Program
    {
        static void Main(string[] args)
        {
            // GA: weight/value problem
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
                System.Console.WriteLine($"Generation {generation}: fitness={solution.Fitness}, solution={solution.Solution}");
            });
        }
    }
}
