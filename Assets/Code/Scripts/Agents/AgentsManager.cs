using PathSystem;
using PathSystem.PathFinding;
using Sirenix.OdinInspector;
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
        [SerializeField] private bool _debug;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [SerializeField] private Node _targetNode;

        [SerializeField] protected bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("_drawGizmos")]
        [SerializeField] private float yOffet = 0.5f;
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

        public static AgentsManager Instance { get; private set; }

        private PathFinder pathFinder;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnEnable()
        {
            ShiftManager.OnEnemyTurn += OnTurnStart;
            // on agent death
        }

        private void OnDisable()
        {
            ShiftManager.OnEnemyTurn -= OnTurnStart;
        }

        #region Main Methods

        private async void OnTurnStart()
        {
            bool result = await CheckRayCast(IdleAgents.Concat(MovingAgents).ToList());

            if (result) return;

            // moving
        }

        private void Attack(Agent agent) { }

        private void Move(List<Agent> agents)
        {
            foreach (Agent agent in agents)
            {
                agent.Index++;

                if (agent.Index > agent.Path.Count)
                {
                    if (agent.IsPatrol)
                    {
                        if (agent.Path[agent.Index--] == _targetNode)
                        {
                            // pathfinding
                        }
                        else
                        {
                            agent.Path.Reverse();
                            agent.Index = 1;
                        }
                    }
                    else
                    {
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
            foreach (Agent agent in agents)
            {
                Ray ray = new(transform.position, transform.forward);

                if (_drawGizmos) Debug.DrawRay(ray.origin, ray.direction * rayDistance, _raylineColor, 1f);

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layerMask))
                {
                    if (_debug) Debug.Log(hit.transform.name);

                    if (hit.transform.CompareTag("Player"))
                    {
                        Attack(agent);
                        return true;
                    }
                }

                await Task.Yield();
            }
            return false;
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
            if(_debug) Debug.Log("New Target");

            foreach (Agent agent in agents)
            {
                Pathfinding(agent, agent.Path[agent.Index], targetNode);

                if (IdleAgents.Contains(agent))
                {
                    MovingAgents.Add(agent);
                    IdleAgents.Remove(agent);
                }
            }
        }

        /// <summary>
        /// Register a new agent
        /// </summary>
        /// <param name="agent"></param>
        public void RegisterAgent(Agent agent)
        {
            if(_debug) Debug.Log("Register agent");

            IdleAgents.Add(agent);
        }

        /// <summary>
        /// Unregister an agent
        /// </summary>
        /// <param name="agent"></param>
        public void UnregisterAgent(Agent agent)
        {
            if(_debug) Debug.Log("Unregister agent");

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
            if (IdleAgents.Contains(agent))
            {
                if (_debug) Debug.Log("Idle to Moving");

                IdleAgents.Remove(agent);
                MovingAgents.Add(agent);
            }
            else
            {
                if (_debug) Debug.Log("Moving to Idle");

                MovingAgents.Remove(agent);
                IdleAgents.Add(agent);
            }
        }

        /// <summary>
        /// Agent changed
        /// </summary>
        /// <param name="agent"></param>
        public void UpdatePath(Agent agent)
        {
            if(_debug) Debug.Log("Agent changed");

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
            if(_debug) Debug.Log("Pathfinding");

            if (startNode == null || targetNode == null)
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
            agent.Path.Clear();
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
                    agent.Path.Insert(0, node.Location);
                    node = node.Parent;
                }
            }
            else
            {
                if (_debug) Debug.LogWarning("Pathfinding failed: No path found.");
            }
        }

        /// <summary>
        /// Finds the furthest connected nodes relative to the agent's current position, 
        /// considering both forward and backward directions along the dominant axis. 
        /// Ensures that the nodes are reachable via their connections.
        /// </summary>
        private void NodeFinder(Agent agent)
        {
            // ======================= normalizzare la rotazione dell'agent

            Vector3 forwardDirection = transform.forward;
            bool isForwardAlongZ = Mathf.Abs(forwardDirection.x) < Mathf.Abs(forwardDirection.z);

            HashSet<Node> visited = new(); // Used to track visited nodes

            Node currentNode = agent.Path[agent.Index];

            // Furthest node forward
            Node nodeForward = FindFurthestConnectedNodeRecursively(agent, currentNode, visited, isForwardAlongZ, false);

            // Furthest node backward
            Node nodeBackward = FindFurthestConnectedNodeRecursively(agent, currentNode, visited, isForwardAlongZ, true);

            Pathfinding(agent, nodeForward, nodeBackward);
        }

        private bool IsAlignedWithAxis(Node node, bool isForwardAlongZ)
        {
            return isForwardAlongZ
                ? Mathf.Approximately(node.transform.position.x, transform.position.x)
                : Mathf.Approximately(node.transform.position.z, transform.position.z);
        }

        private Node FindFurthestConnectedNodeRecursively(Agent agent, Node currentNode, HashSet<Node> visited, bool isForwardAlongZ, bool isForward)
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
                    if (distance > furthestDistance && IsInDirection(agent, neighbour, isForwardAlongZ, isForward))
                    {
                        // Recursively explore the neighbor to find the furthest node
                        Node potentialNode = FindFurthestConnectedNodeRecursively(agent, neighbour, visited, isForwardAlongZ, isForward);

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

        private bool IsInDirection(Agent agent, Node node, bool isForwardAlongZ, bool isForward)
        {
            Node currentNode = agent.Path[agent.Index];

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
            if (_targetNode != null)
            {
                Gizmos.color = _targetNodeColor;
                Gizmos.DrawSphere(PositionNormalize(_targetNode.transform.position), 0.15f);
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
            return new Vector3(position.x, yOffet, position.z);
        }

        #endregion

    }
}
