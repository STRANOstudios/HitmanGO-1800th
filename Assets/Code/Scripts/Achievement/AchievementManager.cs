using Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataSystem
{
    public class AchievementManager : MonoBehaviour
    {
        public string hub;
        public string levelName;
        public string nextLevelName;

        public List<AchievementBase> achievements = new();

        [Header("Debug")]
        [SerializeField] private bool m_debug = false;

        public static event Action OnDataLoading;
        public static event Action OnDataLoaded;
        public static event Action OnDataSaving;
        public static event Action OnDataSaved;

        public static event Action OnDataFailed;

        private LevelData levelData;

        private int counter = 0;
        private bool isCompleted = false;
        private bool isCollected = false;

        private void Awake()
        {
            LoadData();
        }

        private void Start()
        {
            ServiceLocator.Instance.AchievementManager = this;
        }

        private void OnEnable()
        {
            GameManager.OnEndGame += SaveData;

            PlayerController.OnPlayerMove += AddCount;
            GameManager.OnWinCondition += Complete;
            Collectibles.OnCollectibleCollected += OnCollectibleCollected;
        }

        private void OnDisable()
        {
            GameManager.OnEndGame -= SaveData;

            PlayerController.OnPlayerMove -= AddCount;
            GameManager.OnWinCondition -= Complete;
            Collectibles.OnCollectibleCollected -= OnCollectibleCollected;
        }

        private void LoadData()
        {
            OnDataLoading?.Invoke();
            Debug.Log($"Loading level {hub + levelName}");

            if (!SaveSystem.Exists(hub + levelName))
            {
               /* if (m_debug)*/ Debug.Log($"Level {hub + levelName} has not exists.");

                OnDataFailed?.Invoke();
                return;
            }

            levelData = SaveSystem.Load<LevelData>(hub + levelName);

            OnDataLoaded?.Invoke();
            Debug.Log($"Loaded level {hub + levelName}");
        }

        private void SaveData()
        {
            OnDataSaving?.Invoke();

            for (int i = 0; i < levelData.achievements.Count; i++)
            {
                if (achievements[i].CheckCondition())
                    levelData.achievements[i].isCompleted = true;
            }

            SaveSystem.Save(levelData, levelName);

            OnDataSaved?.Invoke();
        }

        #region METHODS FOR ACHIEVEMENT 

        private void AddCount()
        {
            counter++;
        }

        public int GetStepCount => counter;

        private void Complete()
        {
            isCompleted = true;
        }

        public bool GetCompleteState => isCompleted;

        private void OnCollectibleCollected()
        {
            isCollected = true;
        }

        public bool GetCollectibleState => isCollected;

        #endregion
    }
}
