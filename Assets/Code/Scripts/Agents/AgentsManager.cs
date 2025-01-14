using PathSystem;
using PathSystem.PathFinding;
using Player;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Agents
{
    [InitializeOnLoad]
    public class AgentsManager : MonoBehaviour
    {
        [Title("Agents")]
        [SerializeField] private List<Agent> IdleAgents = new();
        [SerializeField] private List<Agent> MovingAgents = new();

        [Title("Settings")]
        [SerializeField] private LayerMask layerMask;
        [SerializeField, Range(0, 5)] private float rayDistance;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [SerializeField] private bool _debugLog = false;

        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [SerializeField] private Node targetNode;

        [SerializeField] protected bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField] private float yOffset = 0.5f;
        // raycast
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _raylineColor = Color.green;
        // destination
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _targetNodeColor = Color.yellow;
        // pathfinder
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _pathColor = Color.red;
        // in move
        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _nextPathColor = Color.magenta;

        // control
        private Node _targetNode = null;

        private static AgentsManager _instance;

        public static AgentsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AgentsManager>();
                }
                return _instance;
            }
        }

        public static event Action OnKillPlayer;

        private PathFinder pathFinder;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeInEditor()
        {
            if (Instance == null)
            {
                AgentsManager existingInstance = FindObjectOfType<AgentsManager>();
                if (existingInstance != null)
                {
                    _instance = existingInstance;
                }
            }
        }
#endif

        private void Awake()
        {
            if (Instance == null) _instance = this;
            else if (Instance != this) DestroyImmediate(this);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                _targetNode = targetNode = null;
                return;
            }

            if (targetNode != null && targetNode != _targetNode)
            {
                _targetNode = targetNode;
                NewTarget(targetNode, IdleAgents.Concat(MovingAgents).ToList());
            }
        }

        private void OnEnable()
        {
            ShiftManager.OnEnemyTurn += OnTurnStart;
            KillHandler.OnKillAgent += OnKill;
        }

        private void OnDisable()
        {
            ShiftManager.OnEnemyTurn -= OnTurnStart;
            KillHandler.OnKillAgent -= OnKill;
        }

        #region Main Methods

        private async void OnTurnStart()
        {
            bool result = await CheckRayCast(IdleAgents.Concat(MovingAgents).ToList());

            Move(MovingAgents);

            if (result)
            {
                OnKillPlayer?.Invoke();
            }
        }

        private void Move(List<Agent> agents)
        {
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                Agent agent = agents[i];

                agent.Index++;

                if (_debugLog) Debug.Log("Index: " + agent.Index + " / " + agent.Path.Count);

                if (agent.Index >= agent.Path.Count)
                {
                    if (_debugLog) Debug.Log("End of path");

                    if (agent.IsPatrol)
                    {
                        if (_debugLog) Debug.Log("isPatrol");

                        if (agent.CurrentNode == targetNode && !agent.HasReachedTarget)
                        {
                            agent.HasReachedTarget = true;

                            if (_debugLog) Debug.Log("Refactoring path");

                            NodeFinder(agent);

                            agent.Index = agent.Path.IndexOf(agent.CurrentNode) + 1;

                            Debug.Log("Index (modified): " + agent.Index);

                            if (agent.Index >= agent.Path.Count - 1)
                            {
                                agent.Path.Reverse();
                                agent.Index = 1;
                            }

                            Debug.Log("Index (modified 2): " + agent.Index);
                        }
                        else
                        {
                            if (_debugLog) Debug.Log("Reversing path");

                            agent.Path.Reverse();
                            agent.Index = 1;
                        }
                    }
                    else
                    {
                        if (_debugLog) Debug.Log("Idle to Moving");

                        agent.StartNode = targetNode;
                        IdleAgents.Add(agent);
                        MovingAgents.Remove(agent);
                        agent.Path.Clear();
                        continue;
                    }
                }

                agent.Move();
            }
        }

        #endregion

        #region Methods

        private async Task<bool> CheckRayCast(List<Agent> agents)
        {
            if (_debugLog) Debug.Log("Raycast");

            foreach (Agent agent in agents)
            {
                Vector3 pos = agent.transform.position;
                pos.y = yOffset;

                Ray ray = new(pos, agent.transform.forward);

                if (_drawGizmos) Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.magenta, 1f);

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layerMask))
                {
                    if (_debugLog) Debug.Log(hit.transform.name);

                    if (hit.transform.CompareTag("Player"))
                    {
                        if (_debugLog) Debug.Log("Player Detected");

                        if (hit.transform.TryGetComponent(out PlayerController component))
                            return component.IsVisible;

                        return true;    
                    }
                }

                await Task.Yield();
            }
            return false;
        }

        private void OnKill(Agent agent)
        {
            if (IdleAgents.Contains(agent))
            {
                IdleAgents.Remove(agent);
            }
            else
            {
                MovingAgents.Remove(agent);
            }
        }

        #endregion

        #region Setter

        /// <summary>
        /// Set a new target
        /// </summary>
        /// <param name="targetNode"></param>
        /// <param name="agents"></param>
        public void NewTarget(Node targetNode, List<Agent> agents)
        {
            if (_debugLog) Debug.Log("New Target");

            _targetNode = this.targetNode = targetNode;

            foreach (Agent agent in agents)
            {
                Node StartNode = agent.StartNode;

                if (!agent.Path.IsNullOrEmpty())
                {
                    StartNode = agent.Path[agent.Index];
                }

                Pathfinding(agent, StartNode, targetNode);

                if (IdleAgents.Contains(agent))
                {
                    MovingAgents.Add(agent);
                    IdleAgents.Remove(agent);
                }

                agent.Index = 0;
                agent.HasReachedTarget = false;
            }
        }

        /// <summary>
        /// Register a new agent
        /// </summary>
        /// <param name="agent"></param>
        public void RegisterAgent(Agent agent)
        {
            if (_debugLog) Debug.Log("Register agent");

            if (agent.IsPatrol)
            {
                if (!MovingAgents.Contains(agent)) MovingAgents.Add(agent);
            }
            else
            {
                if (!IdleAgents.Contains(agent)) IdleAgents.Add(agent);
            }
        }

        /// <summary>
        /// Unregister an agent
        /// </summary>
        /// <param name="agent"></param>
        public void UnregisterAgent(Agent agent)
        {
            if (_debugLog) Debug.Log("Unregister agent");

            if (IdleAgents.Contains(agent))
            {
                IdleAgents.Remove(agent);
            }
            else
            {
                MovingAgents.Remove(agent);
            }
        }

        /// <summary>
        /// Change list
        /// </summary>
        /// <param name="agent"></param>
        public void ChangeList(Agent agent)
        {
            if (agent.IsPatrol)
            {
                if (IdleAgents.Contains(agent))
                {
                    if (_debugLog) Debug.Log("Idle to Moving");

                    IdleAgents.Remove(agent);
                    MovingAgents.Add(agent);
                }
            }
            else
            {
                if (MovingAgents.Contains(agent))
                {
                    if (_debugLog) Debug.Log("Moving to Idle");

                    MovingAgents.Remove(agent);
                    IdleAgents.Add(agent);
                }
            }
        }

        /// <summary>
        /// Agent changed
        /// </summary>
        /// <param name="agent"></param>
        public void UpdatePath(Agent agent)
        {
            if (_debugLog) Debug.Log("Agent changed");

            Pathfinding(agent, agent.StartNode, agent.EndNode);
        }

        #endregion

        #region Pathfinding

        private void Pathfinding(Agent agent, Node startNode, Node targetNode)
        {
            if (!gameObject.activeInHierarchy || !agent.gameObject.activeInHierarchy) return;

            StartCoroutine(CalculatePathCoroutine(agent, startNode, targetNode));
        }

        private IEnumerator CalculatePathCoroutine(Agent agent, Node startNode, Node targetNode)
        {
            if (_debugLog) Debug.Log("Pathfinding");

            // Check if startNode and targetNode are valid
            if (startNode == null || targetNode == null)
            {
                if (_debugLog) Debug.LogWarning("Pathfinding failed: startNode or targetNode is null.");
                yield break;
            }

            // Use A* as the pathfinding algorithm
            pathFinder = new AStarPathFinder
            {
                // Define the cost functions for G and H
                GCostFunction = (a, b) => Vector3.Distance(a.transform.position, b.transform.position), // G cost: distance between nodes
                HCostFunction = (a, b) => Vector3.Distance(a.transform.position, b.transform.position)  // H cost: heuristic (Euclidean distance)
            };

            // Initialize the pathfinder with the start and target nodes
            pathFinder.Initialize(startNode, targetNode);

            // Calculate the path step by step
            agent.Path.Clear();
            while (pathFinder.Status == PathFinderStatus.RUNNING)
            {
                pathFinder.Step(); // Execute one step of the A* algorithm
            }

            // If the path is found, extract the path
            if (pathFinder.Status == PathFinderStatus.SUCCESS)
            {
                PathFinder.PathFinderNode node = pathFinder.CurrentNode;
                while (node != null)
                {
                    agent.Path.Insert(0, node.Location); // Add the node to the path (in reverse order)
                    node = node.Parent; // Move to the parent node
                }

                if (_debugLog) Debug.Log("Path found with " + agent.Path.Count + " nodes.");
            }
            else
            {
                if (_debugLog) Debug.LogWarning("Pathfinding failed: No path found.");
            }
        }

        /// <summary>
        /// Finds the furthest connected nodes relative to the agent's current position, 
        /// considering both forward and backward directions along the dominant axis. 
        /// Ensures that the nodes are reachable via their connections.
        /// </summary>
        private async void NodeFinder(Agent agent)
        {
            Vector3 forwardDirection = agent.transform.forward;
            bool isForwardAlongZ = Mathf.Abs(forwardDirection.x) < Mathf.Abs(forwardDirection.z);

            HashSet<Node> visited = new(); // Used to track visited nodes

            Node currentNode = agent.Path[agent.Index - 1];

            // Trova il nodo più lontano in avanti e indietro
            Node nodeForward = await FindFurthestConnectedNodeRecursivelyAsync(agent, currentNode, visited, isForwardAlongZ, false);
            Node nodeBackward = await FindFurthestConnectedNodeRecursivelyAsync(agent, currentNode, visited, isForwardAlongZ, true);

            // Verifica se è necessario invertire le direzioni
            if (ShouldInvertDirections(agent))
            {
                (nodeBackward, nodeForward) = (nodeForward, nodeBackward);
            }

            if (_debugLog) Debug.Log("nodeForward: " + nodeForward + ", nodeBackward: " + nodeBackward);

            Pathfinding(agent, nodeForward, nodeBackward);
        }

        private async Task<Node> FindFurthestConnectedNodeRecursivelyAsync(Agent agent, Node currentNode, HashSet<Node> visited, bool isForwardAlongZ, bool isForward)
        {
            visited.Add(currentNode);

            if (!IsAlignedWithAxis(agent, currentNode, isForwardAlongZ))
            {
                return null;
            }

            Node furthestNode = currentNode;
            float furthestDistance = 0f;

            await Task.WhenAll(currentNode.neighbours.Select(async neighbour =>
            {
                if (visited.Contains(neighbour) || !IsAlignedWithAxis(agent, neighbour, isForwardAlongZ))
                {
                    return;
                }

                float distance = Vector3.Distance(currentNode.transform.position, neighbour.transform.position);

                if (distance > furthestDistance && IsInDirection(agent, neighbour, isForwardAlongZ, isForward))
                {
                    Node potentialNode = await FindFurthestConnectedNodeRecursivelyAsync(agent, neighbour, visited, isForwardAlongZ, isForward);

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
            }));

            return furthestNode;
        }

        private bool IsAlignedWithAxis(Agent agent, Node node, bool isForwardAlongZ, float tolerance = 0.1f)
        {
            float diff = isForwardAlongZ ? node.transform.position.x - agent.transform.position.x : node.transform.position.z - agent.transform.position.z;
            return Mathf.Abs(diff) < tolerance;
        }

        private bool IsInDirection(Agent agent, Node node, bool isForwardAlongZ, bool isForward, float tolerance = 0.1f)
        {
            Node currentNode = agent.Path[agent.Index - 1];
            float diff = isForwardAlongZ ? node.transform.position.z - currentNode.transform.position.z : node.transform.position.x - currentNode.transform.position.x;
            return isForward ? diff > tolerance : diff < -tolerance;
        }

        private bool ShouldInvertDirections(Agent agent)
        {
            float angle = agent.transform.eulerAngles.y;

            if (angle > 180) angle -= 360;

            float tolerance = 10f;
            return Mathf.Abs(Mathf.Abs(angle) - 180) < tolerance;
        }

        #endregion

        #region Getter

        public int AgentsCount => IdleAgents.Count + MovingAgents.Count;

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            // Ray line
            foreach (Agent agent in IdleAgents.Concat(MovingAgents))
            {
                Gizmos.color = _raylineColor;

                Vector3 pos = agent.transform.position + agent.transform.forward * rayDistance;

                Gizmos.DrawLine(PositionNormalize(agent.transform.position), PositionNormalize(pos));
            }

            // Gizmo target node
            if (targetNode != null)
            {
                Gizmos.color = _targetNodeColor;
                Gizmos.DrawSphere(PositionNormalize(targetNode.transform.position), 0.15f);
            }

            // Gizmo path
            foreach (Agent agent in MovingAgents)
            {
                List<Node> path = agent.Path;

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
        }

        protected Vector3 PositionNormalize(Vector3 position)
        {
            return new Vector3(position.x, yOffset, position.z);
        }

        #endregion

    }
}
