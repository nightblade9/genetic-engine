using GoRogue.MapViews;
using System;

namespace GeneticRoguelike.Model
{
    public class GridMap
    {        
        public const int TILES_WIDE = 80;
        public const int TILES_HIGH = 28;

        internal readonly ArrayMap<bool> Data = new ArrayMap<bool>(GridMap.TILES_WIDE, GridMap.TILES_HIGH);

        private Random random;

        // We create lots of these close to each other temporally, so seed them with a random number from this guy
        private static Random gridMapRandomizer = new Random();

        public GridMap()
        {
            this.random = new Random(gridMapRandomizer.Next());
            this.FillWithRandomTiles();
        }

        public bool Get(int x, int y)
        {
            this.ValidateCoordinates(x, y);
            return Data[x, y];
        }

        public void Set(int x, int y, bool isFloor)
        {
            this.ValidateCoordinates(x, y);
            Data[x, y] = isFloor;
        }

        private void ValidateCoordinates(int x, int y)
        {
            if (x < 0 || y < 0 || x >= TILES_WIDE || y >= TILES_HIGH)
            {
                throw new ArgumentException($"Coordinates ({x}, {y}) are out of range (0, 0) - ({TILES_WIDE}, {TILES_HIGH})");
            }
        }

        internal int SetNRandomTiles(int n, bool newState)
        {
            // Ignore us randomly pickin the same tile more than once, it's rare
            var tilesLeft = n;
            while (tilesLeft > 0)
            {
                var x = random.Next(TILES_WIDE);
                var y = random.Next(TILES_HIGH);
                this.Set(x, y, newState);
                tilesLeft--;
            }
            return 0;
        }

        private void FillWithRandomTiles()
        {
            for (var y = 0; y < TILES_HIGH; y++)
            {
                for (var x = 0; x < TILES_WIDE; x++)
                {
                    this.Set(x, y, random.Next(100) <= 50 ? true : false);
                }
            }
        }
    }
}
