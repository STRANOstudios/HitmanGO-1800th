using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Agents
{
    [InitializeOnLoad]
    public class Agent : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] private Node startNode;

        [SerializeField] private bool isPatrol = false;

        [ShowIf("isPatrol")]
        [SerializeField, Required] private Node endNode;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ReadOnly] public int Index = 0;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] private Node currentNode = null;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ReadOnly] public bool HasReachedTarget = false;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ReadOnly] public List<Node> Path = new();

        public static event Action OnEndMovement;

        // control
        private bool _isPatrol = false;
        private Node _startNode = null;
        private Node _endNode = null;

        private void Awake()
        {
            RegisterToManager();

            currentNode = startNode;
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (_debug) Debug.Log("OnValidate " + gameObject.name);

            if (isPatrol != _isPatrol)
            {
                ChangeToManager();
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
                    AgentsManager.Instance.UpdatePath(this);
                }
            }
        }

        private void OnDestroy()
        {
            AgentsManager.Instance?.UnregisterAgent(this);
        }

        #region Agents Manager

        private void RegisterToManager()
        {
            AgentsManager.Instance.RegisterAgent(this);
        }

        private void ChangeToManager()
        {
            AgentsManager.Instance.ChangeList(this);
        }

        #endregion

        #region Methods

        public void Move()
        {
            currentNode = Path[Index];

            if (_debug) Debug.Log("Moving with Code");
            StartCoroutine(WithCode());
        }

        private IEnumerator WithCode()
        {
            Vector3 targetPosition = Path[Index].transform.position;

            Quaternion targetRotation = CalculateTargetRotation(targetPosition);

            transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
            float moveDuration = 1f;  // Calculate the time to move based on agent speed.
            float elapsedTime = 0f;

            // Perform smooth movement from the start position to the target position.
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveDuration);

                // Move the agent smoothly towards the target position.
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                // Rotate the agent smoothly towards the target position.
                Quaternion smoothedRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                transform.rotation = Quaternion.Euler(0, smoothedRotation.eulerAngles.y, 0);

                yield return null; // Wait for the next frame to continue movement.
            }

            OnEndMovement?.Invoke();
        }

        private float CalculateAngleDifference(Quaternion currentRotation, Quaternion targetRotation)
        {
            float angleDifference = Quaternion.Angle(currentRotation, targetRotation);

            Vector3 cross = Vector3.Cross(currentRotation * Vector3.forward, targetRotation * Vector3.forward);
            if (cross.y < 0) angleDifference = -angleDifference;

            return angleDifference; // -180� | 180�.
        }

        private Quaternion CalculateTargetRotation(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            directionToTarget.y = 0;

            return Quaternion.LookRotation(directionToTarget);
        }

        private float RoundToNearest(float value, float nearest)
        {
            return Mathf.Round(value / nearest) * nearest;
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