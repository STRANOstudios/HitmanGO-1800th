using PathSystem;
using Sirenix.OdinInspector;
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
        [ReadOnly] private Node currentNode = null;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ReadOnly] public List<Node> Path = new();

        private Animator _animator;

        private string _animStep = "Step";
        private string _animDX = "RotationDX";
        private string _animSX = "RotationSX";
        private string _anim180 = "Rotation180";

        // flag
        public bool HasReachedTarget = false;

        // control
        private bool _isPatrol = false;
        private Node _startNode = null;
        private Node _endNode = null; 

        private void Awake()
        {
            RegisterToManager();

            currentNode = startNode;

            if (transform.TryGetComponent(out Animator animator) && animator.isActiveAndEnabled)
            {
                _animator = animator;
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if(_debug) Debug.Log("OnValidate " + gameObject.name);

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

            if(startNode != null)
            {
                if(startNode != _startNode)
                {
                    _startNode = startNode;
                    transform.position = startNode.transform.position;

                    currentNode = startNode;
                }
                
                if(endNode != null && endNode != _endNode)
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

            if (_animator)
            {
                if (_debug) Debug.Log("Moving with Animation");
                StartCoroutine(WithAnimation());
            }
            else
            {
                if (_debug) Debug.Log("Moving with Code");
                StartCoroutine(WithCode());
            }
        }

        private IEnumerator WithAnimation()
        {
            Vector3 targetPosition = Path[Index].transform.position;

            Quaternion targetRotation = CalculateTargetRotation(targetPosition);

            float angleDifference = CalculateAngleDifference(transform.rotation, targetRotation);

            if (Mathf.Abs(angleDifference) > 0)
            {
                if (Mathf.Abs(angleDifference) >= 170)
                {
                    _animator.CrossFadeInFixedTime(_anim180, 0);
                }
                else
                {
                    _animator.CrossFadeInFixedTime(angleDifference < 0 ? _animSX : _animDX, 0);
                }
            }
            else
            {
                _animator.CrossFadeInFixedTime(_animStep, 0);
            }

            yield return null;
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
        }

        private float CalculateAngleDifference(Quaternion currentRotation, Quaternion targetRotation)
        {
            float angleDifference = Quaternion.Angle(currentRotation, targetRotation);

            Vector3 cross = Vector3.Cross(currentRotation * Vector3.forward, targetRotation * Vector3.forward);
            if (cross.y < 0) angleDifference = -angleDifference;

            return angleDifference; // -180° | 180°.
        }

        private Quaternion CalculateTargetRotation(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            directionToTarget.y = 0;

            return Quaternion.LookRotation(directionToTarget);
        }

        #endregion

        #region Getters and Setters

        public bool IsPatrol => isPatrol;
        public Node StartNode => startNode;
        public Node EndNode => endNode;
        public Node CurrentNode => currentNode;

        #endregion

    }
}