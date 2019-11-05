using System.Collections.Generic;
using System.Threading;
using GeneticEngine;
using Microsoft.Xna.Framework;

namespace GeneticRoguelike.Screens
{
    public class VisualizationScreen : SadConsole.Console
    {
        private readonly int STATUS_Y;
        public VisualizationScreen(int width, int height) : base(width, height)
        {
            STATUS_Y = height - 1;

            var evolver = new DungeonEvolver();
            var thread = new Thread(() => evolver.EvolveSolution(this.Redraw));
            thread.Start();
        }

        private void Redraw(int generation, CandidateSolution<List<DungeonOp>> best)
        {
            this.Clear();
            var status = $"Generation {generation}, best fitness = {best.Fitness}, solution is {best.Solution.Count} ops";
            this.Print(0, STATUS_Y, status, Color.White);
            System.Console.WriteLine(status);
        }
    }
}