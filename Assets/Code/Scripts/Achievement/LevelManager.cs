using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achievement
{
    public class LevelManager : MonoBehaviour
    {
        private AchievementManager achievementManager;

        void Start()
        {
            achievementManager = FindObjectOfType<AchievementManager>();
        }

        // Method to be called when the player completes a level
        public void CompleteLevel(int levelNumber, int turnsTaken, bool killedTarget, bool killedEnemies, bool retrieveX, bool killedAnyone)
        {
            // Example for achievement unlocking based on conditions

            // Level-specific achievements
            Achievement finishLevel = new Achievement("FINISH_LEVEL", "Complete the level");
            achievementManager.UnlockAchievement(finishLevel);

            switch (levelNumber)
            {
                case 6:
                    if (killedEnemies) achievementManager.UnlockAchievement(new Achievement("KILL_ALL_ENEMIES", "Kill all enemies"));
                    if (!killedAnyone) achievementManager.UnlockAchievement(new Achievement("DONT_KILL_ANYONE", "Don't kill anyone"));
                    break;

                case 7:
                    if (turnsTaken <= 5) achievementManager.UnlockAchievement(new Achievement("FINISH_IN_5_TURNS", "Finish in 5 turns or less"));
                    if (!killedAnyone) achievementManager.UnlockAchievement(new Achievement("DONT_KILL_ANYONE", "Don't kill anyone"));
                    break;

                case 8:
                    if (killedTarget) achievementManager.UnlockAchievement(new Achievement("KILL_TARGET", "Kill the target"));
                    if (turnsTaken <= 14) achievementManager.UnlockAchievement(new Achievement("FINISH_14_TURNS", "Finish in 14 turns or less"));
                    if (retrieveX) achievementManager.UnlockAchievement(new Achievement("RETRIEVE_X", "Retrieve x"));
                    break;

                // Other cases for other levels...

                default:
                    break;
            }
        }
    }
}
