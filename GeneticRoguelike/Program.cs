using System;
using GeneticRoguelike.Model;
using GeneticEngine;

namespace GeneticRoguelike
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine<List<DungeonOp>, GridMap>();
            engine.Solve();
        }
    }
}
