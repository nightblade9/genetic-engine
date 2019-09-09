using System;
using System.Collections.Generic;
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
            engine.SetMutationMethod(Mutate);
            engine.Solve();
        }

        // Not part of the engine because it doesn't know if we want a tree, list, etc.
        private List<DungeonOp> CreateRandomDungeonOpList()
        {
            var length = random.Next(5, 16);
            var toReturn = new List<DungeonOp>();
            while (toReturn.Count < length)
            {
                var newOp = DungeonOp.CreateRandom();
                toReturn.Add(newOp);
            }
            return toReturn;
        }

        private List<DungeonOp> Mutate(List<DungeonOp> input)
        {
            var toReturn = new List<DungeonOp>(input);
            var mutationOp = random.Next(100);

            if (mutationOp < 30) // 30%
            {
                toReturn.Add(DungeonOp.CreateRandom());
            }
            else if (mutationOp >= 30 && mutationOp < 60) // 30%
            {
                var index = random.Next(toReturn.Count);
                toReturn.RemoveAt(index);
            }
            else // 40%
            {
                var firstIndex = random.Next(toReturn.Count);
                var secondIndex = random.Next(toReturn.Count);
                // Swap. Don't care if they're the same
                var temp = toReturn[firstIndex];
                toReturn[firstIndex] = toReturn[secondIndex];
                toReturn[secondIndex] = temp;
            }

            return toReturn;
        }
    }
}