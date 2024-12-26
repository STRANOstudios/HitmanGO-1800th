using PathSystem;
using PathSystem.PathFinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    [DisallowMultipleComponent]
    public class AgentFSM : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] protected Node currentNode;
        [SerializeField] private bool _isPatrol = false;
        [SerializeField, ShowIf("_isPatrol"), Required] protected Node _targetNode;

        [SerializeField] protected float speed = 1.0f;
        [SerializeField] protected float raycastDistance = 1.0f;
        [SerializeField] protected LayerMask raycastMask;

        #region Debug

        [Title("Debug")]
        [SerializeField] protected bool _debug = false;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] protected Node _targetNodeDebug;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] protected List<Node> path = new();

        [SerializeField, ShowIf("_debug")] protected bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField] protected float yOffet = 0.5f;
        // raycast
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] protected Color _raylineColor = Color.green;
        // destination
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] protected Color _targetNodeColor = Color.yellow;
        // pathfinder
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] protected Color _pathColor = Color.red;
        // in move
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] protected Color _nextPathColor = Color.magenta;

        #endregion

        protected bool ShowGizmos => _drawGizmos && _debug;

        protected FSMInterface _currentState;

        private PathFinder pathFinder;

        private void Start()
        {
            _currentState = new Idle(this);
        }

        private void OnValidate()
        {
            if (_isPatrol && _targetNode != null)
            {
                Pathfinding();
            }
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

        }

        #region Setters

        public Node SetTargetNode
        {
            set
            {
                _targetNode = value;
                Pathfinding();
            }
        }

        #endregion

        #region Gizmos

        protected void OnDrawGizmos()
        {
            if (!ShowGizmos) return;

            Gizmos.color = _raylineColor;

            Vector3 pos = transform.position + transform.forward * raycastDistance;

            Gizmos.DrawLine(PositionNormalize(transform.position), PositionNormalize(pos));

            if (_targetNode != null)
            {
                Gizmos.color = _targetNodeColor;
                Gizmos.DrawSphere(PositionNormalize(_targetNode.transform.position), 0.15f);
            }

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