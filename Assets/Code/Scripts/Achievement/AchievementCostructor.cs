using DataSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementCostructor : MonoBehaviour
{
    public List<Hub> hubLevelDatas = new();

    [Button]
    public void AddAchievement()
    {
        Inizialized();
    }

    private void Awake()
    {
        if (!SaveSystem.Exists(hubLevelDatas[0].hub + hubLevelDatas[0].levelDatas[0].levelID))
        {
            Debug.Log("Inizializing");
            Inizialized();
        }
    }

    private void Inizialized()
    {
        foreach (Hub hublevelData in hubLevelDatas)
        {
            foreach (LevelData levelData in hublevelData.levelDatas)
            {
                SaveSystem.Save(levelData, hublevelData.hub + levelData.levelID);
            }
        }
    }

    [Serializable]
    public class Hub
    {
        public string hub;
        public List<LevelData> levelDatas = new();
    }
}
