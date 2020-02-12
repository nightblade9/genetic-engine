using GeneticEngine;
using SampleSolutions.Model;
using SampleSolutions.Screens;
using SadConsole;

namespace SampleSolutions
{
    class Program
    {
        static void Main(string[] args)
        {
            // Not a good idea, fitness is non-deterministic.
            RunRoguelikeProblem();

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
            // new CurveFittingSolver().EvolveSolution((generation, solution) => 
            // {
            //     System.Console.WriteLine($"Generation {generation}: fitness={solution.Fitness}");
            // });
        }

        private static void RunRoguelikeProblem()
        {
            SadConsole.Game.Create("Fonts/IBM.font", GridMap.TILES_WIDE, GridMap.TILES_HIGH);

            SadConsole.Game.OnInitialize = () =>
            {
                Global.CurrentScreen = new VisualizationScreen(GridMap.TILES_WIDE, GridMap.TILES_HIGH);
            };

            SadConsole.Game.OnDestroy = () =>
            {
                (Global.CurrentScreen as VisualizationScreen).ShutDown();
            };

            SadConsole.Game.Instance.Run();

            SadConsole.Game.Instance.Dispose();
        }
    }
}
