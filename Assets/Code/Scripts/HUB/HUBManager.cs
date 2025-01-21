using DataSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HUB
{
    public class HUBManager : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private string HUBName = "1";
        [SerializeField] private List<LevelDataHUB> Levels = new();

        [SerializeField] private GameObject achievementIndicator;

        [SerializeField, ColorPalette] private Color m_challengeCompletedColor = Color.white;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [ShowIf("_debug")]
        [SerializeField]
        private List<GameObject> activeObjects = new();

        private LevelData levelData;

        public static event Action OnDataLoaded;

        private void Awake()
        {
            if (Levels.Count == 0) return;

            foreach (LevelDataHUB level in Levels)
            {
                level.button.SetActive(false);
            }

            CheckUnlockLevel();
        }

        private void CheckUnlockLevel()
        {
            if (Levels.Count == 0) return;

            foreach (LevelDataHUB level in Levels)
            {
                if (!SaveSystem.Exists(HUBName + level.levelName))
                {
                    if (_debug) Debug.Log($"Level {level.levelName} has not exists.");
                    continue;
                }

                LevelData levelData = SaveSystem.Load<LevelData>(HUBName + level.levelName);

                if(!levelData.isUnlocked) continue;

                level.button.SetActive(true);

                foreach (var achievement in levelData.achievements)
                {
                    if (achievementIndicator == null) continue;

                    var indicator = Instantiate(achievementIndicator, level.achievementContainer.transform);

                    if (achievement.isCompleted)
                        indicator.GetComponent<Image>().color = m_challengeCompletedColor;
                }

                if (levelData.isUnlocked)
                    activeObjects.AddRange(level.root);
            }

            OnDataLoaded?.Invoke();
        }

        public List<GameObject> ActiveObjects => activeObjects;
    }

    [Serializable]
    public class LevelDataHUB
    {
        public string levelName;
        public GameObject button;
        public GameObject achievementContainer;
        public List<GameObject> root;
    }
}
