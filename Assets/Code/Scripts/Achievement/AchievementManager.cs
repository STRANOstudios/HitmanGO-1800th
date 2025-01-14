using System.Collections.Generic;
using UnityEngine;

namespace Achievement
{
    public class AchievementManager : MonoBehaviour
    {
        private List<Achievement> unlockedAchievements = new List<Achievement>();
        private LocalizationManager localizationManager;

        void Start()
        {
            localizationManager = FindObjectOfType<LocalizationManager>();
        }

        // Method to unlock an achievement
        public void UnlockAchievement(Achievement achievement)
        {
            if (!unlockedAchievements.Contains(achievement))
            {
                unlockedAchievements.Add(achievement);
                string localizedName = localizationManager.GetLocalizedText(achievement.NameKey);
                Debug.Log($"Achievement Unlocked: {localizedName}");
                // Here you could update the UI or trigger a sound
            }
        }

        // Check if an achievement has been unlocked
        public bool IsAchievementUnlocked(Achievement achievement)
        {
            return unlockedAchievements.Contains(achievement);
        }

        // Reset all achievements
        public void ResetAchievements()
        {
            unlockedAchievements.Clear();
            Debug.Log("All achievements have been reset.");
        }
    }
}
