using System;
using GeneticRoguelike.Model;
using GeneticEngine;

namespace GeneticRoguelike
{
    public class DungeonEvolver
    {
        private Random random = new Random();

        public void EvolveSolution()
        {
            var engine = new Engine<List<DungeonOp>, GridMap>();
            engine.CreateInitialPopulation(CreateRandomDungeonOpList);
            engine.Solve();
        }

        // Not part of the engine because it doesn't know if we want a tree, list, etc.
        private List<DungeonOp> CreateInitialPopulation()
        {
            var length = random.Next(5, 16);
            var toReturn = new List<DungeonOp>();
            while (toReturn.Count < length)
            {
                var newOp = DungeonOp.CreateRandom();
                toReturn.Add(newOp);
            }
        }
    }
}