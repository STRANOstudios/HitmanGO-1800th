using PathSystem;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Node m_startNode;

        private Node currentNode;

        private void Awake()
        {
            currentNode = m_startNode;

            Debug.Log("Current Node: " + currentNode.name);

            transform.position = currentNode.transform.position;
        }

        public Node CurrentNode => currentNode;
    }
}
