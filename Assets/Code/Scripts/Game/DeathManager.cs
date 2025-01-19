using Agents;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class DeathManager : MonoBehaviour
    {
        [SerializeField] List<Transform> m_deathPositions = new();
        [SerializeField, Range(0, 10f)] private float m_heightSpawn = 5f;

        private int m_deathCount = 0;

        private void Awake()
        {
            ServiceLocator.Instance.DeathManager = this;
        }

        private void OnEnable()
        {
            ServiceLocator.OnServiceLoactorCreated += OnServerInitialized;
        }

        private void OnDisable()
        {
            ServiceLocator.OnServiceLoactorCreated += OnServerInitialized;
        }

        private void OnServerInitialized()
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
        }
    }
}
