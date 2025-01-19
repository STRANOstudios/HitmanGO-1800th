using PathSystem;
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
        [SerializeField] private string HUBName = "Level";
        [SerializeField] private List<LevelData> Levels = new();

        [SerializeField, ColorPalette] private Color m_challengeLockedColor = Color.gray;
        [SerializeField, ColorPalette] private Color m_challengeCompletedColor = Color.white;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [ShowIf("_debug")]
        [SerializeField]
        private List<GameObject> activeObjects = new();

        private bool[] unlocked;

        public static event Action OnDataLoaded;

        private void Awake()
        {
            if (Levels.Count == 0) return;

            foreach (LevelData level in Levels)
            {
                level.button.SetActive(false);
            }

            CheckUnlockLevel();
        }

        private void OnValidate()
        {
            foreach (LevelData level in Levels)
            {
                SaveSystem.Save(level, HUBName + level.levelName);
            }
        }

        private void CheckUnlockLevel()
        {
            if (Levels.Count == 0) return;

            foreach (LevelData level in Levels)
            {
                if (!SaveSystem.Exists(HUBName + level.levelName))
                {
                    if (_debug) Debug.Log($"Level {level.levelName} has not exists.");
                    return;
                }

                LevelData levelData = SaveSystem.Load<LevelData>(HUBName + level.levelName);

                level.button.SetActive(true);

                level.unlocked = levelData.unlocked;

                for (int i = 0; i < level.challengeSprites.Count; i++)
                {
                    if (level.challengeSprites[i] != null)
                        level.challengeSprites[i].color = i < level.challengesCompleted ? m_challengeCompletedColor : m_challengeLockedColor;
                }

                if (level.unlocked)
                    activeObjects.AddRange(level.root);
            }

            OnDataLoaded?.Invoke();
        }

        public List<GameObject> ActiveObjects => activeObjects;
    }

    [Serializable]
    public class LevelData
    {
        public string levelName;
        public GameObject button;
        public List<Image> challengeSprites;
        public List<GameObject> root;

        public bool unlocked = false;
        public int challengesCompleted = 0;
    }
}
