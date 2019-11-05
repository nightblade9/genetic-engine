using Microsoft.Xna.Framework;

namespace GeneticRoguelike.Screens
{
    public class VisualizationScreen : SadConsole.Console
    {
        public VisualizationScreen(int width, int height) : base(width, height)
        {
            this.Print(3, 3, "Hello, world!", Color.White);
        }
    }
}