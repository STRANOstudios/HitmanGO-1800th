using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HUB
{
    public class HUBManager : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private string HUBName = "Level";
        [SerializeField] private List<LevelData> Levels = new();

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [ShowIf("_debug")] 
        [SerializeField]
        private List<GameObject> activeObjects = new();

        private bool[] unlocked;


        private void Awake()
        {
            if(Levels.Count == 0) return;

            foreach (LevelData level in Levels)
            {
                level.button.SetActive(false);
            }

            CheckUnlockLevel();
        }

        private void Start()
        {

        }

        private void CheckUnlockLevel()
        {
            if (Levels.Count == 0) return;

            //LevelData levelData = SaveSystem.Load<LevelData>(HUBName); // leggo i dati salvati

            if (/*levelData != null*/false)
            {
                
            }
            else
            {
                Levels[0].unlocked = true;
            }
        }

        public List<GameObject> ActiveObjects => activeObjects;
    }

    [Serializable]
    public class LevelData
    {
        public GameObject button;
        public List<Sprite> sprites;
        public List<Node> root;
        public bool unlocked = false;
    }
}
