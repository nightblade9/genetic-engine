using GoRogue.MapViews;
using System;

namespace GeneticRoguelike.Model
{
    public class GridMap
    {        
        public const int TILES_WIDE = 80;
        public const int TILES_HIGH = 28;
        private const int ROOM_MIN_SIZE = 4;
        private const int ROOM_MAX_SIZE = 6;
        private const int HALL_MIN_SIZE = 5;
        private const int HALL_MAX_SIZE = 8;

        // false = can't walk
        internal readonly ArrayMap<bool> Data = new ArrayMap<bool>(GridMap.TILES_WIDE, GridMap.TILES_HIGH);

        private Random random;

        // We create lots of these close to each other temporally, so seed them with a random number from this guy
        private static Random gridMapRandomizer = new Random();

        public GridMap()
        {
            this.random = new Random(gridMapRandomizer.Next());
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

        #region dungeon ops
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

        internal void SetHallway(bool newState, bool isHorizontal)
        {
            var size = random.Next(HALL_MIN_SIZE, HALL_MAX_SIZE);
            var startX = random.Next(isHorizontal ? TILES_WIDE - size : TILES_WIDE);
            var startY = random.Next(isHorizontal ? TILES_HIGH : TILES_HIGH - size);

            int stopX = isHorizontal ? startX + size : startX;
            int stopY = isHorizontal ? startY : startY + size;

            for (var y = startY; y <= stopY; y++)
            {
                for (int x = startX; x <= stopX; x++)
                {
                    this.Set(x, y, newState);
                }
            }
        }

        internal void SetArea(bool newState)
        {
            var width = random.Next(ROOM_MIN_SIZE, ROOM_MAX_SIZE);
            var height = random.Next(ROOM_MIN_SIZE, ROOM_MAX_SIZE);
            
            var startX = random.Next(TILES_WIDE - width);
            var startY = random.Next(TILES_HIGH - height);

            for (var y = startY; y < startY + height; y++)
            {
                for (var x = startX; x < startX + width; x++)
                {
                    this.Set(x, y, newState);
                }
            }
        }

        internal void SetRandomWalk(int numSteps, bool newState)
        {
            var x = random.Next(TILES_WIDE);
            var y = random.Next(TILES_HIGH);
            var iterations = 1000;

            while (iterations-- > 0 && numSteps > 0)
            {
                if (this.Get(x, y) != newState)
                {
                    this.Set(x, y, newState);
                    numSteps--;
                }

                double next = random.NextDouble();
                if (next < 0.25f)
                {
                    x -= 1;
                }
                else if (next < 0.5)
                {
                    x += 1;
                }
                else if (next < 0.75)
                {
                    y -= 1;
                }
                else
                {
                    y += 1;
                }

                if (x < 0 || x >= TILES_WIDE || y < 0 || y >= TILES_HIGH)
                {
                    // walked off the edge. Just quit.
                    iterations = 0;
                }
            }
        }

        internal void Smooth()
        {
            var newData = new bool[TILES_WIDE, TILES_HIGH];
            for (var y = 1; y < TILES_HIGH - 1; y++)
            {
                for (var x = 1; x < TILES_WIDE - 1; x++)
                {
                    var numSolids = 0;
                    numSolids += this.Get(x - 1, y - 1) == false ? 1 : 0;
                    numSolids += this.Get(x, y - 1) == false ? 1 : 0;
                    numSolids += this.Get(x + 1, y - 1) == false ? 1 : 0;
                    numSolids += this.Get(x - 1, y) == false ? 1 : 0;
                    numSolids += this.Get(x + 1, y) == false ? 1 : 0;
                    numSolids += this.Get(x - 1, y + 1) == false ? 1 : 0;
                    numSolids += this.Get(x, y + 1) == false ? 1 : 0;
                    numSolids += this.Get(x + 1, y + 1) == false ? 1 : 0;

                    var newState = numSolids >= 5 ? false : true;
                    newData[x, y] = newState;
                }
            }

            for (var y = 1; y < TILES_HIGH - 1; y++)
            {
                for (var x = 1; x < TILES_WIDE - 1; x++)
                {
                    this.Set(x, y, newData[x, y]);
                }
            }
        }

#endregion

        private void ValidateCoordinates(int x, int y)
        {
            if (x < 0 || y < 0 || x >= TILES_WIDE || y >= TILES_HIGH)
            {
                throw new ArgumentException($"Coordinates ({x}, {y}) are out of range (0, 0) - ({TILES_WIDE}, {TILES_HIGH})");
            }
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
