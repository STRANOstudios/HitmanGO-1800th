using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    public class Agent : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] private Node startNode;

        [SerializeField] private bool isPatrol = false;

        [ShowIf("isPatrol")]
        public int Index = 0;

        [ShowIf("isPatrol")]
        [SerializeField, Required] private Node endNode;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;

        [ShowIfGroup("_debug")]

        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Node currentNode = null;

        [ShowIfGroup("_debug")]
        [ReadOnly] public bool HasReachedTarget = false;

        public List<Node> Path = new();

        public static event Action OnEndMovement;

        // control
        private bool _isPatrol = false;
        private Node _startNode = null;
        private Node _endNode = null;

        private void Start()
        {
            ServiceLocator.Instance.AgentsManager.RegisterAgent(this);

            currentNode = startNode;

            currentNode.Storages.Add(gameObject);
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (_debug) Debug.Log("OnValidate " + gameObject.name);

            if (isPatrol != _isPatrol)
            {
                _isPatrol = isPatrol;

                if (!_isPatrol)
                {
                    Path.Clear();
                    endNode = null;
                    Index = 0;
                }
            }

            if (startNode != null)
            {
                if (startNode != _startNode)
                {
                    _startNode = startNode;
                    transform.position = startNode.transform.position;

                    currentNode = startNode;
                }

                if (endNode != null && endNode != _endNode)
                {
                    _endNode = endNode;
                    Debug.DrawLine(endNode.transform.position, endNode.transform.position + Vector3.up, Color.red, 1f);
                }
            }

            Index = Mathf.Clamp(Index, 0, Mathf.Max(0, Path.Count - 1));

            if (Index != 0)
            {
                currentNode = Path[Index];
                transform.position = currentNode.transform.position;
            }
        }

        #region Methods

        public void Move()
        {
            Utils.NodeInteraction(currentNode, Path[Index], gameObject);
            currentNode = Path[Index];

            OnEndMovement?.Invoke();
        }

        /// <summary>
        /// Unregister the agent from the manager. 
        /// And register in Death Manager.
        /// </summary>
        public void Death()
        {
            currentNode.Storages.Remove(gameObject);

            ServiceLocator.Instance.AgentsManager.UnregisterAgent(this);

            ServiceLocator.Instance.DeathManager.RegisterAgent(this);
        }

        #endregion

        #region Getters and Setters

        public bool IsPatrol => isPatrol;

        public Node StartNode
        {
            get => startNode;
            set
            {
                startNode = value;

                if (Application.isPlaying) return;
                transform.position = startNode.transform.position;
            }
        }

        public Node EndNode => endNode;

        public Node CurrentNode => currentNode;

        #endregion

    }
}