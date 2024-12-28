using PathSystem;
using System.Collections;
using UnityEngine;

namespace Agents
{
    public class Move : FSMInterface
    {
        private AgentFSM _agent;
        private int _currentPathIndex;
        private bool _isPatrolling;
        private bool _isMoving;  // Flag to check if the agent is currently moving.

        public Move(AgentFSM agent)
        {
            _agent = agent;
            _currentPathIndex = 1;
            _isPatrolling = _agent._isPatrol;
            _isMoving = false;  // Initially, the agent is not moving.
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

            Node targetNode = _agent.path[_currentPathIndex];
            _agent.currenNode = targetNode;
            Vector3 targetPosition = targetNode.transform.position;

            // Start the movement coroutine.
            _agent.StartCoroutine(MoveToNode(targetPosition, targetNode));
        }

        private IEnumerator MoveToNode(Vector3 targetPosition, Node targetNode)
        {
            // Set the moving flag to true since we are starting movement.
            _isMoving = true;

            _agent.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
            float moveDuration = 4.5f / _agent.speed;  // Calculate the time to move based on agent speed.
            float elapsedTime = 0f;

            Vector3 directionToTarget = (targetPosition - _agent.transform.position).normalized;
            directionToTarget.y = 0; // Ignore vertical component.

            // Create a target rotation around the Y-axis only.
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

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

            // Once the movement is complete, increment the path index.
            _currentPathIndex++;

            if (_agent.InPatrol && _currentPathIndex >= _agent.path.Count)
            {
                if (_agent.path.Count > 1)
                {
                    _agent.path.Reverse();
                    _currentPathIndex = 1;
                }
                else
                {
                    if (_agent._debug) Debug.LogWarning("Move state: Path is too short for patrolling.");
                    _agent._currentState = new Idle(_agent);
                }
            }

            if (_currentPathIndex >= _agent.path.Count)
            {
                if (_agent._debug) Debug.Log("Move state: Target reached.");

                _agent.path.Clear();

                // Se l'agente non è in patrolling, fermati
                if (!_agent.InPatrol)
                {
                    _agent._currentState = new Idle(_agent); // Passa allo stato Idle.
                }

                yield break; // Termina la coroutine.
            }

            // Once the movement is complete, set the moving flag to false.
            _isMoving = false;
        }
    }
}
