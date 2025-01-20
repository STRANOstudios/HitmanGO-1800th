using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Agents
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class Agent : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] private Node startNode;

        [SerializeField] private bool isPatrol = false;

        [ShowIf("isPatrol")]
        [SerializeField, Required] private Node endNode;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;

        [ShowIfGroup("_debug")]
        [ReadOnly] public int Index = 0;

        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Node currentNode = null;

        [ShowIfGroup("_debug")]
        [ReadOnly] public bool HasReachedTarget = false;

        [ShowIfGroup("_debug")]
        [ReadOnly] public List<Node> Path = new();

        public static event Action OnEndMovement;

        // control
        private bool _isPatrol = false;
        private Node _startNode = null;
        private Node _endNode = null;

        private void Start()
        {
            ServiceLocator.Instance.AgentsManager.RegisterAgent(this);
            if (isPatrol) ServiceLocator.Instance.AgentsManager.UpdatePath(this);

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
                    ServiceLocator.Instance.AgentsManager.UpdatePath(this);
                }
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
                transform.position = startNode.transform.position;
            }
        }

        public Node EndNode => endNode;

        public Node CurrentNode => currentNode;

        #endregion

    }
}