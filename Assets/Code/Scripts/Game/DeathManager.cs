using Agents;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class DeathManager : MonoBehaviour
    {
        [SerializeField] AgentsManager m_agentsManager = null;
        [SerializeField] List<Transform> m_deathPositions = new();
        [SerializeField, Range(0, 10f)] private float m_heightSpawn = 5f;

        private int m_deathCount = 0;

        private void Start()
        {
            if (m_agentsManager.AgentsCount < m_deathPositions.Count)
            {
                Debug.LogError("Not enough death positions");
            }
        }

        private void OnEnable()
        {
            KillHandler.OnKillAgent += GetDeathBody;
        }

        private void OnDisable()
        {
            KillHandler.OnKillAgent -= GetDeathBody;
        }

        private void GetDeathBody(Agent agent)
        {
            Vector3 pos = m_deathPositions[m_deathCount].position;
            pos.y = m_heightSpawn;
            agent.transform.position = pos;

            m_deathCount++;
        }

    }
}
