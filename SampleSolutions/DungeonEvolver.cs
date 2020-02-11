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
        private const int DUNGEON_WIDTH = 80;
        private const int DUNGEON_HEIGHT = 28;
        private const int NUMBER_OF_POINTS_TO_CALCULATE = 10;
                
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

            // Perhaps the most costly part of all: generate a dungeon, pick ten random points, and calculate the 
            // average distance (walking a path) from each point to each every point (10 * 9).

            // Generate ten points. Repeats are ignored.
            var points = new List<GoRogue.Coord>(NUMBER_OF_POINTS_TO_CALCULATE);
            int iterations = 0;
            while (iterations++ < 10000 && points.Count < points.Capacity)
            {
                int x = 0;
                int y = 0;

                // Since we're doing this from a Parallel.ForEach, if we don't lock on random,
                // it keeps returning zero; we end up with 10k iterations checking (0, 0) every time.
                //  There doesn't seem to be much performance penalty to doing this.
                lock (randomLock)
                {
                    x = random.Next(GridMap.TILES_WIDE);
                    y = random.Next(GridMap.TILES_HIGH);
                }

                var next = new GoRogue.Coord(x, y);
                // Make sure we get walkable points
                if (map.Get(x, y) == true && !points.Contains(next))
                {
                    points.Add(next);
                }
            }

            // Calculate distance from each point to each point
            var numCalculated = 0;
            var totalCalculated = 0f;
            var aStar = new AStar(map.Data, GoRogue.Distance.MANHATTAN);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    var path = aStar.ShortestPath(points[i], points[j]);
                    numCalculated++;
                    totalCalculated +=  path != null ? path.Length : 0; // 0 if no path found
                }
            }

            var average = numCalculated == 0 ? 0 : totalCalculated / numCalculated;
            return average;
        }
    }
}