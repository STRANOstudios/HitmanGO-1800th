using PathSystem;
using PathSystem.PathFinding;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Agents
{
    [DisallowMultipleComponent]
    public class AgentFSM : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] protected Node startNode;
        [SerializeField] public bool _isPatrol = false;
        [SerializeField, ShowIf("_isPatrol"), Required] protected Node _targetNode;

        [SerializeField] public float speed = 1.0f;
        [SerializeField] protected float raycastDistance = 1.0f;
        [SerializeField] protected LayerMask raycastMask;

        #region Debug

        [Title("Debug")]
        [SerializeField] public bool _debug = false;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [SerializeField] protected Node _targetNodeDebug;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] public List<Node> path = new();

        [SerializeField] protected bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField] protected float yOffet = 0.5f;
        // raycast
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] protected Color _raylineColor = Color.green;
        // destination
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] protected Color _targetNodeColor = Color.yellow;
        // pathfinder
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] protected Color _pathColor = Color.red;
        // in move
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] protected Color _nextPathColor = Color.magenta;

        #endregion

        // Flags
        public bool InPatrol;

        public FSMInterface _currentState;
        private PathFinder pathFinder;

        private Coroutine pathfindingCoroutine;

        public Node currentNode;
        private Node lastStartNode = null, lastTargetNode = null;

        private void Start()
        {
            _currentState = new Idle(this);

            if (_isPatrol)
            {
                if (_targetNode == null && _debug)
                {
                    Debug.LogError("Target node is null, please assign a target node.");
                }
                else
                {
                    _currentState = new Move(this);
                }
            }
        }

        private void OnValidate()
        {
            if (_isPatrol && _targetNode != null)
            {
                Pathfinding();
            }

            InPatrol = _isPatrol;
        }

        private void OnEnable()
        {
            ShiftManager.OnEnemyTurn += Turn;
        }

        private void OnDisable()
        {
            ShiftManager.OnEnemyTurn -= Turn;
        }

        private void Turn()
        {
            if (IsPlayerInSight())
            {
                _currentState = new Attack(this);
            }

            if (_targetNode != null)
            {
                _currentState.Update();
            }

            if (IsPlayerInSight())
            {
                _currentState = new Attack(this);
            }
        }

        public bool IsPlayerInSight()
        {
            Ray ray = new(transform.position, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, raycastMask))
            {
                if (_debug) Debug.Log(hit.transform.name);

                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;
        }

        private void Pathfinding()
        {
            if (!gameObject.activeInHierarchy) return;

            if (pathfindingCoroutine != null)
            {
                StopCoroutine(pathfindingCoroutine);
            }

            pathfindingCoroutine = StartCoroutine(CalculatePathCoroutine());
        }

        private IEnumerator CalculatePathCoroutine()
        {
            // If the start node or the target node has changed, recalculate the path
            //if (startNode == lastStartNode && _targetNode == lastTargetNode) yield break;

            lastStartNode = startNode;
            lastTargetNode = _targetNode;

            if (startNode == null || _targetNode == null)
            {
                if (_debug) Debug.LogWarning("Pathfinding failed: currentNode or _targetNode is null.");
                yield break;
            }

            // Usa Dijkstra come algoritmo
            pathFinder = new DijkstraPathFinder
            {
                // Definisci le funzioni di costo per G e H
                GCostFunction = (a, b) => Vector3.Distance(a.transform.position, b.transform.position),
                HCostFunction = (a, b) => 0 // Per Dijkstra, H è sempre 0
            };

            // Inizializza il pathfinder
            pathFinder.Initialize(startNode, _targetNode);

            // Calcola il percorso passo dopo passo
            path.Clear();
            while (pathFinder.Status == PathFinderStatus.RUNNING)
            {
                pathFinder.Step();
                yield return null;
            }

            // Se il percorso è stato trovato, estrai il cammino
            if (pathFinder.Status == PathFinderStatus.SUCCESS)
            {
                PathFinder.PathFinderNode node = pathFinder.CurrentNode;
                while (node != null)
                {
                    path.Insert(0, node.Location);
                    node = node.Parent;
                }
            }
            else
            {
                if (_debug) Debug.LogWarning("Pathfinding failed: No path found.");
            }

            pathfindingCoroutine = null; // Reset 
        }

        /// <summary>
        /// Finds the furthest connected nodes relative to the agent's current position, 
        /// considering both forward and backward directions along the dominant axis. 
        /// Ensures that the nodes are reachable via their connections.
        /// </summary>
        public void NodeFinder()
        {
            Vector3 forwardDirection = transform.forward;
            bool isForwardAlongZ = Mathf.Abs(forwardDirection.x) < Mathf.Abs(forwardDirection.z);

            HashSet<Node> visited = new(); // Used to track visited nodes

            // Furthest node forward
            Node nodeForward = FindFurthestConnectedNodeRecursively(
                currentNode, visited, isForwardAlongZ, false);

            // Furthest node backward
            Node nodeBackward = FindFurthestConnectedNodeRecursively(
                currentNode, visited, isForwardAlongZ, true);

            _targetNode = nodeBackward;
            startNode = nodeForward;

            InPatrol = true;
            Pathfinding();
            
            _currentState = new Move(this);
        }

        private bool IsAlignedWithAxis(Node node, bool isForwardAlongZ)
        {
            return isForwardAlongZ
                ? Mathf.Approximately(node.transform.position.x, transform.position.x)
                : Mathf.Approximately(node.transform.position.z, transform.position.z);
        }

        private Node FindFurthestConnectedNodeRecursively(Node currentNode, HashSet<Node> visited, bool isForwardAlongZ, bool isForward)
        {
            // Add the current node to the visited nodes
            visited.Add(currentNode);

            // Make sure the node is aligned with the correct axis
            if (!IsAlignedWithAxis(currentNode, isForwardAlongZ))
                return null; // If it's not aligned, we don't consider it

            // Check if there are unvisited neighbors
            Node furthestNode = currentNode;
            float furthestDistance = 0f;

            foreach (Node neighbour in currentNode.neighbours)
            {
                // Don't explore nodes we've already visited
                if (visited.Contains(neighbour))
                    continue;

                // If the node is aligned and hasn't been visited, explore it
                if (IsAlignedWithAxis(neighbour, isForwardAlongZ))
                {
                    // Calculate the distance from the current node
                    float distance = Vector3.Distance(currentNode.transform.position, neighbour.transform.position);

                    // Check if this node is farther and in the right direction
                    if (distance > furthestDistance && IsInDirection(neighbour, isForwardAlongZ, isForward))
                    {
                        // Recursively explore the neighbor to find the furthest node
                        Node potentialNode = FindFurthestConnectedNodeRecursively(neighbour, visited, isForwardAlongZ, isForward);

                        // If we find a further node, update the furthest node
                        if (potentialNode != null)
                        {
                            float potentialDistance = Vector3.Distance(currentNode.transform.position, potentialNode.transform.position);
                            if (potentialDistance > furthestDistance)
                            {
                                furthestNode = potentialNode;
                                furthestDistance = potentialDistance;
                            }
                        }
                    }
                }
            }

            // Return the furthest node found
            return furthestNode;
        }

        private bool IsInDirection(Node node, bool isForwardAlongZ, bool isForward)
        {
            if (isForwardAlongZ)
            {
                return isForward
                    ? node.transform.position.z > currentNode.transform.position.z
                    : node.transform.position.z < currentNode.transform.position.z;
            }
            else
            {
                return isForward
                    ? node.transform.position.x > currentNode.transform.position.x
                    : node.transform.position.x < currentNode.transform.position.x;
            }
        }

        private bool IsNodeConnected(Node node, HashSet<Node> visitedNodes)
        {
            foreach (var neighbour in node.neighbours)
            {
                if (!visitedNodes.Contains(neighbour))
                {
                    if (_debug) Debug.DrawLine(node.transform.position + Vector3.up, node.transform.position + Vector3.up * 2, Color.green, 3f);
                    return true;
                }
            }
            if (_debug) Debug.DrawLine(node.transform.position + Vector3.up, node.transform.position + Vector3.up * 2, Color.red, 3f);
            return false;
        }

        #region Setters

        /// <summary>
        /// Set the target node. If the agent is in patrol, it stops patrolling.
        /// </summary>
        public Node SetTargetNode
        {
            set
            {
                _targetNode = value;

                if (currentNode != null)
                {
                    startNode = currentNode;
                }

                if (_isPatrol && value != null)
                {
                    // Stop patrolling when a specific target is assigned.
                    InPatrol = false;
                }

                if (_debug) Debug.Log($"Target node set to {_targetNode.name}. Patrolling stopped: {!_isPatrol}");

                // Start pathfinding to the target node.
                Pathfinding();

                // Change the state to Move, as the agent needs to move to the target.
                _currentState = new Move(this);
            }
        }

        #endregion

        #region Gizmos

        protected void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            // Gizmo Raycast
            Gizmos.color = _raylineColor;

            Vector3 pos = transform.position + transform.forward * raycastDistance;

            Gizmos.DrawLine(PositionNormalize(transform.position), PositionNormalize(pos));

            // Gizmo target node
            if (_targetNode != null)
            {
                Gizmos.color = _targetNodeColor;
                Gizmos.DrawSphere(PositionNormalize(_targetNode.transform.position), 0.15f);
            }

            // Gizmo path
            if (path.Count > 0)
            {
                Color _patrolPathColorEnds = Color.Lerp(_pathColor, Color.magenta, 0.5f);

                Gizmos.color = _patrolPathColorEnds;

                Gizmos.DrawSphere(PositionNormalize(path[0].transform.position), 0.15f);

                if (path.Count > 1)
                {
                    Gizmos.DrawSphere(PositionNormalize(path[^1].transform.position), 0.15f);

                    Gizmos.color = _pathColor;

                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        Gizmos.DrawLine(PositionNormalize(path[i].transform.position), PositionNormalize(path[i + 1].transform.position));
                        if (i > 0 && i < path.Count - 1) Gizmos.DrawSphere(PositionNormalize(path[i].transform.position), 0.1f);
                    }
                }

            }
        }

        protected Vector3 PositionNormalize(Vector3 position)
        {
            return new Vector3(position.x, yOffet, position.z);
        }

        #endregion
    }
}