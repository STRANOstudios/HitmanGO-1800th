using Agents;
using PathSystem;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactables
{
    public class Distractor : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private string m_nameTargets;
        [SerializeField] private List<Node> nodes = new();

        [Title("Settings")]
        [SerializeField, Required] private Node m_node;
        [SerializeField] private Vector3 m_sizeBounds = Vector3.one;

        [Title("Debug")]
        [SerializeField] private bool _drawGizmos = true;

        [FoldoutGroup("Color settings"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color m_boundsColor = Color.red;

        [FoldoutGroup("Color settings"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color m_positionColor = Color.red;

        [Button]
        public void StartCheck() => Spawn();

        private List<GameObject> indicators = new();

        private Node target;
        private bool isInteracting = false;

        public static event Action OnInteractEnd;

        private void Awake()
        {
            transform.position = m_node.transform.position;
            m_node.Storages.Add(gameObject);
        }

        private void Start()
        {
            if (nodes.Count > 0) return;

            // avvia controllo
            nodes = Utils.CheckGameObjectsInBox(
                transform.position,
                m_sizeBounds,
                NodeCache.Nodes
                );
        }

        private void OnValidate()
        {
            if (m_node.transform.position != transform.position)
            {
                transform.position = m_node.transform.position;
            }
        }

        public void Spawn()
        {
            foreach (Node node in nodes)
            {
                if (!node.Storages.IsNullOrEmpty())
                    continue;

                GameObject gameObject = ObjectPooler.Instance.Get(m_nameTargets);
                gameObject.transform.position = node.transform.position;

                DistractorCollider collider = gameObject.GetComponent<DistractorCollider>();
                collider.distractor = this;
                collider.node = node;

                indicators.Add(gameObject);
            }
        }

        private void Check()
        {
            m_node.Storages.Remove(gameObject);

            ServiceLocator.Instance.AgentsManager.SetTarget(
                target,
                Utils.CheckGameObjectsInBox(
                    target.transform.position,
                    m_sizeBounds,
                    ServiceLocator.Instance.AgentsManager.Agents)
                );

            for (int i = 0; i < indicators.Count; i++)
            {
                indicators[i].GetComponent<DistractorCollider>().Clear();
                ObjectPooler.Instance.ReturnToPool(indicators[i]);
            }

            indicators.Clear();

            OnInteractEnd?.Invoke();

            gameObject.SetActive(false);
        }

        public Node SetTarget
        {
            set
            {
                target = value;
                Check();
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            Gizmos.color = m_boundsColor;
            Gizmos.DrawWireCube(isInteracting ? target.transform.position : transform.position, m_sizeBounds);

            foreach (Node node in nodes)
            {
                Gizmos.color = m_positionColor;
                Gizmos.DrawWireSphere(node.transform.position, 0.15f);
            }
        }
    }
}
