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
            new DungeonOp("Set5RandomTiles", (gridMap) => gridMap.SetNRandomTiles(5, true)),
            new DungeonOp("Clear5RandomTiles", (gridMap) => gridMap.SetNRandomTiles(5, false)),
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