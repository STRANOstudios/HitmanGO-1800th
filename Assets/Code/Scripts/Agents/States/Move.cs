using PathSystem;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Agents
{
    public class Move : FSMInterface
    {
        private AgentFSM _agent;
        private int _currentPathIndex;
        private bool _isPatrolling;
        private bool _isMoving;  // Flag to check if the agent is currently moving.

        // Animation
        private Animator _animator = null;

        private string _animIdle = "Idle";
        private string _animStep = "Step";
        private string _animDX = "RotationDX";
        private string _animSX = "RotationSX";
        private string _anim180 = "Rotation180";

        private bool _setted = false;

        public Move(AgentFSM agent)
        {
            _agent = agent;
            _currentPathIndex = 1;

            _isPatrolling = _agent._isPatrol;
            _isMoving = false;  // Initially, the agent is not moving.

            if (_agent.TryGetComponent(out Animator animator) && animator.isActiveAndEnabled) _animator = animator;
        }

        public void Enter()
        {
            if (_agent._debug) Debug.Log("Move state: Enter.");
        }

        public void Exit() { }

        public void Reset() { }

        // Called once per update to start the movement when it's the agent's turn.
        public void Update()
        {
            if (_isMoving || _agent.path.Count == 0)
                return; // If the agent is already moving or the path is empty, do nothing.

            // If there is a path to follow, start the movement.
            if (_agent.path.Count > 0)
            {
                StartMoving();
            }
        }

        private void StartMoving()
        {
            // Check if there is a node to move to.
            if (_agent.path.Count == 0)
                return;

            if (_agent.InPatrol && !_setted)
            {
                _currentPathIndex = _agent.path.IndexOf(_agent.currentNode) + 1;
                _setted = true;
            }

            if (_currentPathIndex < 1) _currentPathIndex = 1;
            if (_currentPathIndex >= _agent.path.Count) _currentPathIndex = _agent.path.Count - 1;

            if (_agent.InPatrol && _currentPathIndex >= _agent.path.Count)
            {
                if (_agent.path.Count > 1)
                {
                    _agent.path.Reverse();  // Reverse the path to go back
                    _currentPathIndex = 1;  // Skip the first node as we are already at it
                }
                else
                {
                    if (_agent._debug) Debug.LogWarning("Move state: Path is too short for patrolling.");
                    _agent._currentState = new Idle(_agent);
                }
            }

            ClearLog();
            Debug.Log("========= Path =========");

            // Usa LINQ per creare una lista dei nomi corretti direttamente e concatenarli.
            string pathList = string.Join(", ", _agent.path.Select(path => path.name.Split(' ').Last()));

            Debug.Log("Path List: [" + pathList + "]");
            Debug.Log("Current Path Index: " + _currentPathIndex);
            Debug.Log("Current Node: " + _agent.path[_currentPathIndex].name);

            Node targetNode = _agent.path[_currentPathIndex];
            _agent.currentNode = targetNode;
            Vector3 targetPosition = targetNode.transform.position;

            // Start the movement coroutine.
            _agent.StartCoroutine(MoveToNode(targetPosition));
        }

        private IEnumerator MoveToNode(Vector3 targetPosition)
        {
            // Set the moving flag to true since we are starting movement.
            _isMoving = true;

            Vector3 directionToTarget = (targetPosition - _agent.transform.position).normalized;
            directionToTarget.y = 0; // Ignore vertical component.

            // Create a target rotation around the Y-axis only.
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            if (!_animator)
            {
                if (_agent._debug) Debug.Log("No animator found, using Move Animation Programmed.");

                #region Move Animation Programmed

                _agent.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
                float moveDuration = 4.5f / _agent.speed;  // Calculate the time to move based on agent speed.
                float elapsedTime = 0f;

                // Perform smooth movement from the start position to the target position.
                while (elapsedTime < moveDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / moveDuration);

                    // Move the agent smoothly towards the target position.
                    _agent.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                    // Rotate the agent smoothly towards the target position.
                    Quaternion smoothedRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                    _agent.transform.rotation = Quaternion.Euler(0, smoothedRotation.eulerAngles.y, 0);

                    yield return null; // Wait for the next frame to continue movement.
                }

                #endregion
            }
            else
            {
                if (_agent._debug) Debug.Log("Animator found, using Move Animation with Animator.");

                #region Move Animation with Animator

                float angleDifference = CalculateAngleDifference(_agent.transform.rotation, targetRotation);

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

                #endregion
            }

            _currentPathIndex++;

            if (_agent._debug)
            {
                Debug.Log($"Current Path Index: {_currentPathIndex}, Path Count: {_agent.path.Count}");
                Debug.DrawLine(targetPosition, targetPosition + Vector3.up, _agent._targetNodeColor, 1f);
            }

            // If path index exceeds the path count, we have reached the target or end of the path.
            if (_currentPathIndex >= _agent.path.Count)
            {
                if (_agent._debug) Debug.Log("Move state: Target reached.");

                _agent.path.Clear();

                if (!_agent._isPatrol)
                {
                    _agent._currentState = new Idle(_agent);
                }
                else
                {
                    if (!_agent.InPatrol)
                    {
                        _agent.NodeFinder(); // Recalculate the path.
                    }
                }

                yield break; // End the movement.
            }

            // Once the movement is complete, set the moving flag to false.
            _isMoving = false;
        }

        private float CalculateAngleDifference(Quaternion currentRotation, Quaternion targetRotation)
        {
            float angleDifference = Quaternion.Angle(currentRotation, targetRotation);

            Vector3 cross = Vector3.Cross(currentRotation * Vector3.forward, targetRotation * Vector3.forward);
            if (cross.y < 0) angleDifference = -angleDifference;

            return angleDifference; // -180° | 180°.
        }

#if UNITY_EDITOR
        public void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
#endif
    }
}
