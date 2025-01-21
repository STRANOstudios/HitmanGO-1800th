using DataSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class AchievementCostructor : MonoBehaviour
{
    public string hub;
    public LevelData levelData;

    [Button]
    public void AddAchievement()
    {
        
        SaveSystem.Save(levelData, hub + levelData.levelID);
    }
}
