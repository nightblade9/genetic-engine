using System;
using System.Collections.Generic;
using SampleSolutions.Model;
using GeneticEngine;
using GoRogue.Pathing;

namespace SampleSolutions
{
    public class DungeonEvolver
    {
        private const int MINIMUM_SOLUTION_SIZE = 3; // accept no less than 3 nodes
                
        // Lock access to the RNG because it's expensive to create and not thread-safe; if accessed
        // from mutliple threads, Next() just returns 0 all the time (which is why we see best=0).
        // See: https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.8
        private Object randomLock = new Object();
        private Random random = new Random();

        public void EvolveSolution(Action<int, CandidateSolution<List<DungeonOp>>> callback)
        {
            var engine = new Engine<List<DungeonOp>, GridMap>(1000, 0.1f, 0.95f, 0.05f);
            engine.CreateInitialPopulation(this.CreateRandomDungeonOpList);
            engine.SetFitnessMethod(this.CalculateFitness);
            engine.SetCrossOverMethod(this.CrossOver);
            engine.SetSelectionMethod(engine.TournamentSelection);
            engine.SetMutationMethod(this.Mutate);
            engine.SetOnGenerationCallback(callback);
            engine.Solve();
        }

        // Not part of the engine because it doesn't know if we want a tree, list, etc.
        private List<DungeonOp> CreateRandomDungeonOpList()
        {
            var length = random.Next(40, 50);
            var toReturn = new List<DungeonOp>();
            while (toReturn.Count < length)
            {
                var newOp = DungeonOp.CreateRandom();
                toReturn.Add(newOp);
            }
            return toReturn;
        }

        private void Mutate(List<DungeonOp> input)
        {
            var mutationOp = random.Next(100);

            if (mutationOp < 50) // add a random op
            {
                var op = DungeonOp.CreateRandom();
                var index = random.Next(input.Count);
                input.Insert(index, op);
            }
            else if (mutationOp >= 33 && mutationOp < 66) // swap two elements
            {
                var firstIndex = random.Next(input.Count);
                var secondIndex = random.Next(input.Count);
                // Swap. Don't care if they're the same
                var temp = input[firstIndex];
                input[firstIndex] = input[secondIndex];
                input[secondIndex] = temp;
            }
            else // remove random element
            {
                if (input.Count > MINIMUM_SOLUTION_SIZE)
                {
                    var index = random.Next(input.Count);
                    input.RemoveAt(index);
                }
            }
        }

        private List<List<DungeonOp>> CrossOver(List<DungeonOp> parent1, List<DungeonOp> parent2)
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

            return new List<List<DungeonOp>>() { child1, child2 };
        }

        private float CalculateFitness(List<DungeonOp> solution)
        {
            var map = new GridMap();
            foreach (DungeonOp op in solution)
            {
                op.Execute(map);
            }

            // Calculate the walking distance from every point to the center. Manhatten distance (no sqrt).
            // More distance is better, obviously, because we're more maze-like.
            var center = new GoRogue.Coord(GridMap.TILES_WIDE / 2, GridMap.TILES_HIGH / 2);
            map.Set(center.X, center.Y, true);

            var totalCalculated = 0f;

            var aStar = new AStar(map.Data, GoRogue.Distance.MANHATTAN);

            for (var y = 0; y < GridMap.TILES_HIGH; y++)
            {
                for (var x = 0; x < GridMap.TILES_WIDE; x++)
                {
                    var path = aStar.ShortestPath(new GoRogue.Coord(x, y), center);
                    totalCalculated +=  path != null ? path.Length : 0; // 0 if no path found
                }
            }

            return totalCalculated;
        }
    }
}