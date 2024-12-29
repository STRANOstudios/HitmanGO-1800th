using PathSystem;
using PathSystem.PathFinding;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

        public Node currentNode;

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
            if (startNode == null || _targetNode == null)
            {
                if (_debug) Debug.LogWarning("Pathfinding failed: currentNode or _targetNode is null.");
                return;
            }

            // Usa Dijkstra come algoritmo
            pathFinder = new DijkstraPathFinder
            {
                // Definisci le funzioni di costo per G e H
                GCostFunction = (a, b) => Vector3.Distance(a.transform.position, b.transform.position),
                HCostFunction = (a, b) => 0 // Per Dijkstra, H � sempre 0
            };

            // Inizializza il pathfinder
            pathFinder.Initialize(startNode, _targetNode);

            // Calcola il percorso passo dopo passo
            path.Clear();
            while (pathFinder.Status == PathFinderStatus.RUNNING)
            {
                pathFinder.Step();
            }

            // Se il percorso � stato trovato, estrai il cammino
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
        }

        /// <summary>
        /// Search the edge nodes in forward and backward
        /// </summary>
        public void NodeFinder()
        {
            List<Node> nodes = new(FindObjectsOfType<Node>());

            // Calcoliamo la direzione forward dell'agente (vettore normalizzato)
            Vector3 forwardDirection = transform.forward;

            // Determiniamo l'asse di ricerca in base alla direzione di forward
            bool isForwardAlongZ = Mathf.Abs(forwardDirection.x) < Mathf.Abs(forwardDirection.z);

            // Filtra i nodi per il loro asse di ricerca (X o Z)
            IEnumerable<Node> filteredNodes = nodes
                .Where(node => isForwardAlongZ
                    ? Mathf.Approximately(node.transform.position.x, transform.position.x) // Se lungo Z, stessi X
                    : Mathf.Approximately(node.transform.position.z, transform.position.z)); // Se lungo X, stessi Z

            // Funzione per verificare se un nodo � raggiungibile dal currentNode
            Node FindFurthestConnectedNode(IEnumerable<Node> candidates, bool isForward)
            {
                Node current = currentNode;
                Node furthestNode = currentNode;
                float furthestDistance = 0f;

                HashSet<Node> visited = new();
                Queue<Node> queue = new();
                queue.Enqueue(current);
                visited.Add(current);

                while (queue.Count > 0)
                {
                    Node node = queue.Dequeue();

                    // Verifica se il nodo corrente � tra i candidati e calcola la distanza
                    if (candidates.Contains(node))
                    {
                        float distance = Vector3.Distance(currentNode.transform.position, node.transform.position);
                        if (distance > furthestDistance)
                        {
                            // Controlla la direzione
                            if (isForward)
                            {
                                if (isForwardAlongZ && node.transform.position.z > currentNode.transform.position.z ||
                                    !isForwardAlongZ && node.transform.position.x > currentNode.transform.position.x)
                                {
                                    furthestNode = node;
                                    furthestDistance = distance;
                                }
                            }
                            else
                            {
                                if (isForwardAlongZ && node.transform.position.z < currentNode.transform.position.z ||
                                    !isForwardAlongZ && node.transform.position.x < currentNode.transform.position.x)
                                {
                                    furthestNode = node;
                                    furthestDistance = distance;
                                }
                            }
                        }
                    }

                    // Aggiungi i vicini non visitati
                    foreach (Node neighbour in node.neighbours)
                    {
                        if (!visited.Contains(neighbour))
                        {
                            visited.Add(neighbour);
                            queue.Enqueue(neighbour);
                        }
                    }
                }

                return furthestNode;
            }

            // Cerca il nodo pi� distante avanti
            Node nodeForward = FindFurthestConnectedNode(filteredNodes, true);

            // Cerca il nodo pi� distante indietro
            Node nodeBackward = FindFurthestConnectedNode(filteredNodes, false);

            // Assegna targetNode e startNode
            _targetNode = nodeBackward;
            startNode = nodeForward;

            InPatrol = true;
            Pathfinding();
            _currentState = new Move(this);
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