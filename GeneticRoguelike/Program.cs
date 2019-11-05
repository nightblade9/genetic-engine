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
            SadConsole.Game.Create("Fonts/IBM.font", WIDTH, HEIGHT);

            SadConsole.Game.OnInitialize = () => {
                Global.CurrentScreen = new VisualizationScreen(WIDTH, HEIGHT);
            };

            SadConsole.Game.Instance.Run();

            SadConsole.Game.Instance.Dispose();
        }
    }
}
