using System;
using System.Collections.Generic;

namespace DataSystem
{
    [Serializable]
    public class LevelData
    {
        public int levelID;
        public bool isUnlocked;
        public bool isCompleted;
        public List<string> achievements;

        public LevelData(int id, bool unlocked, bool completed)
        {
            levelID = id;
            isUnlocked = unlocked;
            isCompleted = completed;
            achievements = new List<string>();
        }
    }
}
