using System;
using System.Collections.Generic;

namespace DataSystem
{
    [Serializable]
    public class GameData
    {
        public List<LevelData> levels;

        public GameData()
        {
            levels = new();
        }
    }
}
