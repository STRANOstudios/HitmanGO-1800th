using PathSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Player.VFX
{
    public class PlayerMoveIndicator : MonoBehaviour
    {
        [SerializeField] private bool _debugLog = false;

        private GameObject m_prefabsToCreate;
        private float m_distance;

        private List<GameObject> m_indicators = new();

        private PlayerController PlayerController;

        private void Start()
        {
            PlayerController = GetComponent<PlayerController>();

            PathDesign style = FindAnyObjectByType<StyleManager>()?.PathDesign;
            m_prefabsToCreate = style.playerIndicator;
            m_distance = style.PlayerIndicatorDistance;
        }

        private void OnEnable()
        {
            ShiftManager.OnPlayerTurn += Active;
            PlayerController.OnPlayerMove += DeActivate;
        }

        private void OnDisable()
        {
            ShiftManager.OnPlayerTurn -= Active;
            PlayerController.OnPlayerMove -= DeActivate;
        }

        private void Active()
        {
            if (_debugLog) Debug.Log("Active");

            Node currentNode = PlayerController.CurrentNode;

            foreach (var neighbour in currentNode.neighbours)
            {
                GameObject indicator = ObjectPooler.Instance.Get(m_prefabsToCreate.name);

                indicator.transform.position = currentNode.transform.position;

                Vector3 direction = (neighbour.transform.position - currentNode.transform.position).normalized;

                indicator.transform.position += direction * m_distance;

                indicator.transform.LookAt(neighbour.transform.position);

                m_indicators.Add(indicator);
            }
        }

        private void DeActivate()
        {
            if (_debugLog) Debug.Log("DeActivate");

            foreach (var indicator in m_indicators)
            {
                ObjectPooler.Instance.ReturnToPool(indicator);
            }

            m_indicators.Clear();
        }
    }
}
