using Agents;
using Audio;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class DeathManager : MonoBehaviour
    {
        [SerializeField] List<Transform> m_deathPositions = new();
        [SerializeField, Range(0, 10f)] private float m_heightSpawn = 5f;

        public string m_clipName = "";

        private int m_deathCount = 0;

        private void Awake()
        {
            ServiceLocator.Instance.DeathManager = this;
        }

        private void OnEnable()
        {
            ServiceLocator.OnServiceLocatorCreated += OnInitialized;
        }

        private void OnDisable()
        {
            ServiceLocator.OnServiceLocatorCreated += OnInitialized;
        }

        private void OnInitialized()
        {
            if (ServiceLocator.Instance.AgentsManager.AgentsCount < m_deathPositions.Count)
            {
                Debug.LogError("Not enough death positions");
            }
        }

        public void RegisterAgent(Agent agent)
        {
            Vector3 pos = m_deathPositions[m_deathCount].position;
            pos.y = m_heightSpawn;
            agent.transform.position = pos;

            m_deathCount++;

            AudioManager.Instance.PlaySFX(m_clipName);
        }

        public bool IsFull() => m_deathCount >= m_deathPositions.Count;

        public bool IsEmpty() => m_deathCount == 0;
    }
}
