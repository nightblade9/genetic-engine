using GeneticRoguelike.Screens;
using SadConsole;

namespace GeneticRoguelike
{
    class Program
    {
        private const int WIDTH = 80;
        private const int HEIGHT = 29;
        
        static void Main(string[] args)
        {
            RunRoguelikeProblem();

            // POC: weight/value problem
            //new WeightValueSolver().EvolveSolution((generation, solution) => System.Console.WriteLine($"Generation {generation}: fitness={solution.Fitness}"));
        }

        private static void RunRoguelikeProblem()
        {
            SadConsole.Game.Create("Fonts/IBM.font", WIDTH, HEIGHT);

            SadConsole.Game.OnInitialize = () =>
            {
                Global.CurrentScreen = new VisualizationScreen(WIDTH, HEIGHT);
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
