using System;
using System.Collections.Generic;

namespace DataSystem
{
    [Serializable]
    public class LevelData
    {
        public int levelID;
        public bool isUnlocked;
        public List<AchievementData> achievements;

        public LevelData(int id, bool unlocked)
        {
            levelID = id;
            isUnlocked = unlocked;
            achievements = new List<AchievementData>();
        }

        public LevelData() { }
    }

    [Serializable]
    public class AchievementData
    {
        public bool isCompleted;
    }
}
