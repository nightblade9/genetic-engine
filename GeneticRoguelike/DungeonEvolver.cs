using System;
using System.Collections.Generic;
using GeneticRoguelike.Model;
using GeneticEngine;

namespace GeneticRoguelike
{
    public class DungeonEvolver
    {
        private const int MINIMUM_SOLUTION_SIZE = 3; // accept no less than 3 nodes
        private Random random = new Random();

        public void EvolveSolution()
        {
            var engine = new Engine<List<DungeonOp>, GridMap>();
            engine.CreateInitialPopulation(CreateRandomDungeonOpList);
            engine.SetCrossOverMethod(CrossOver);
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
                if (toReturn.Count > MINIMUM_SOLUTION_SIZE)
                {
                    var index = random.Next(toReturn.Count);
                    toReturn.RemoveAt(index);
                }
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

        private Tuple<List<DungeonOp>, List<DungeonOp>> CrossOver(List<DungeonOp> parent1, List<DungeonOp> parent2)
        {
            // Ah, DungeonOp, ah; we have two lists of different sizes. So pick a point in the middle of each list (not the first/last),
            // and then swap everything. So if we have ABCDEFGH and 1234, and we pick F and 2, we end up with ABCDE234 and 1FGH
            var parent1Index = random.Next(1, parent1.Count - 1);
            var parent2Index = random.Next(1, parent2.Count - 1);

            var p1Start = parent1.GetRange(0, parent1Index); // ABCDE
            var p1End = parent2.GetRange(parent2Index, parent2.Count - parent2Index); // 234

            var p2Start = parent2.GetRange(0, parent2Index); // 1
            var p2End = parent1.GetRange(parent1Index, parent1.Count - parent1Index); // FGH

            var child1 = new List<DungeonOp>(p1Start);
            child1.AddRange(p1End);

            var child2 = new List<DungeonOp>(p2Start);
            child2.AddRange(p2End);

            return new Tuple<List<DungeonOp>, List<DungeonOp>>(child1, child2);
        }
    }
}