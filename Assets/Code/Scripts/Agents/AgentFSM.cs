using PathSystem;
using PathSystem.PathFinding;
using Sirenix.OdinInspector;
using System.Collections.Generic;
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

        public Node currenNode;

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
                HCostFunction = (a, b) => 0 // Per Dijkstra, H è sempre 0
            };

            // Inizializza il pathfinder
            pathFinder.Initialize(startNode, _targetNode);

            // Calcola il percorso passo dopo passo
            path.Clear();
            while (pathFinder.Status == PathFinderStatus.RUNNING)
            {
                pathFinder.Step();
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

                if(currenNode != null)
                {
                    startNode = currenNode;
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