using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Interactables
{
    public class Distractor : MonoBehaviour, IInteractable
    {
        [Title("Settings")]
        [SerializeField] private Node m_node;
        [SerializeField] private List<Node> nodes = new();

        private void Awake()
        {
            transform.position = m_node.transform.position;
            m_node.Storages.Add(gameObject);
        }

        public void Interact()
        {
            
        }

        public void InteractEnd()
        {
            
        }
    }
}
