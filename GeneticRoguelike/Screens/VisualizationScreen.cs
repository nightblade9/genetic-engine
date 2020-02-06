using System;
using System.Collections.Generic;
using System.Threading;
using GeneticEngine;
using GeneticRoguelike.Model;
using Microsoft.Xna.Framework;

namespace GeneticRoguelike.Screens
{
    public class VisualizationScreen : SadConsole.Console
    {
        private readonly Color WALL_COLOUR = new Color(128, 128, 128);
        private readonly Color FLOOR_COLOUR = new Color(64, 64, 64);
        private readonly int STATUS_Y;
        private Thread thread;
        private DateTime startedOn;
        private Object drawLock = new Object();

        public VisualizationScreen(int width, int height) : base(width, height)
        {
            STATUS_Y = height - 1;

            var evolver = new DungeonEvolver();
            this.thread = new Thread(() => evolver.EvolveSolution(this.Redraw));
            this.thread.Start();
            startedOn = DateTime.Now;
        }

        public void ShutDown()
        {
            this.thread.Suspend();
        }

        private void Redraw(int generation, CandidateSolution<List<DungeonOp>> bestSolution)
        {
            var fitness = bestSolution.Fitness;
            var data = new List<DungeonOp>(bestSolution.Solution);

            var map = new GridMap();
            foreach (var op in data)
            {
                op.Execute(map);
            }

            lock (drawLock)
            {
                this.Clear();
                
                var status = $"{(DateTime.Now - this.startedOn).TotalSeconds}s | Generation {generation}, FITNESS: best={fitness}, with {data.Count} ops";
                this.Print(0, STATUS_Y, status, Color.White);
                System.Console.WriteLine(status);

                for (var y = 0; y < this.Height - 1; y++)
                {
                    for (var x = 0; x < this.Width; x++)
                    {
                        var text = map.Get(x, y) ? "." : "#";
                        var colour = text == "#" ? WALL_COLOUR : FLOOR_COLOUR;
                        this.Print(x, y, text, colour);
                    }
                }
            }
        }
    }
}