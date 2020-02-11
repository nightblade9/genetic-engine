using System;
using System.Collections.Generic;
using GeneticRoguelike.Model;

namespace GeneticRoguelike
{
    public class DungeonOp
    {
        public static Random random = new Random();
        
        private string name = "";
        private Action<GridMap> callback;

        public static List<DungeonOp> ALL_OPS = new List<DungeonOp>()
        {
            //new DungeonOp("Set5RandomTiles", (gridMap) => gridMap.SetNRandomTiles(5, false)),
            new DungeonOp("Clear5RandomTiles", (gridMap) => gridMap.SetNRandomTiles(5, true)),
            //new DungeonOp("SetHorizontalHallway", (gridMap) => gridMap.SetHallway(false, true)),
            //new DungeonOp("SetVerticalHallway", (gridMap) => gridMap.SetHallway(false, false)),
            //new DungeonOp("ClearHorizontalHallway", (gridMap) => gridMap.SetHallway(true, true)),
            //new DungeonOp("ClearVerticalHallway", (gridMap) => gridMap.SetHallway(true, false)),
            //new DungeonOp("SetRoom", (gridMap) => gridMap.SetArea(false)),
            new DungeonOp("ClearRoom", (gridMap) => gridMap.SetArea(true)),
            new DungeonOp("Clear5RandomWalk", (gridMap) => gridMap.SetRandomWalk(5, true)),
            new DungeonOp("Clear10RandomWalk", (gridMap) => gridMap.SetRandomWalk(10, true)),
            new DungeonOp("Clear20RandomWalk", (gridMap) => gridMap.SetRandomWalk(20, true)),
            new DungeonOp("Set5RandomWalk", (gridMap) => gridMap.SetRandomWalk(5, false)),
            //new DungeonOp("Smooth", (gridMap) => gridMap.Smooth()),
        };

        public static DungeonOp CreateRandom()
        {
            var opIndex = random.Next(ALL_OPS.Count);
            return ALL_OPS[opIndex];    
        }

        public DungeonOp(string name, Action<GridMap> callback)
        {
            this.name = name;
            this.callback = callback;
        }

        public void Execute(GridMap target)
        {
            this.callback.Invoke(target);
        }

        override public string ToString()
        {
            return this.name;
        }
    }
}