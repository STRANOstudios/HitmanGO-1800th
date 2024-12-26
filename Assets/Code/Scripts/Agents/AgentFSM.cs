using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Agents
{
    [DisallowMultipleComponent]
    public class AgentFSM : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] protected Node currentNode;

        [SerializeField] protected float speed = 1.0f;
        [SerializeField] protected float raycastDistance = 1.0f;
        [SerializeField] protected LayerMask raycastMask;

        [Title("Debug")]
        [SerializeField, PropertyOrder(1)] protected bool _debug = false;
        [FoldoutGroup("Debug"), ShowIf("_debug"), PropertyOrder(2)]
        [ShowInInspector, ReadOnly] protected Node targetNode;
        [FoldoutGroup("Debug"), ShowIf("_debug"), PropertyOrder(3)]
        [ShowInInspector, ReadOnly] protected List<Node> path;

        [SerializeField, ShowIf("_debug"), PropertyOrder(4)] protected bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos"), PropertyOrder(5)]
        [SerializeField] protected float yOffet = 0.5f;
        // destination
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos"), PropertyOrder(6)]
        [SerializeField, ColorPalette] protected Color _targetNodeColor = Color.yellow;
        // pathfinder
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos"), PropertyOrder(7)]
        [SerializeField, ColorPalette] protected Color _pathColor = Color.red;
        // in move
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos"), PropertyOrder(8)]
        [SerializeField, ColorPalette] protected Color _nextPathColor = Color.magenta;

        protected bool ShowGizmos => _drawGizmos && _debug;

        protected FSMInterface currentState;

        private void Start()
        {

        }

        private void OnEnable()
        {
            // reach the turn signal
        }

        private void OnDisable()
        {

        }

        private void myTurn()
        {
            // before move


            // check if see player
            Ray ray = new(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, raycastMask))
            {
                if (_debug) Debug.Log(hit.transform.name);

                // after see player
            }
        }

        #region Gizmos

        protected void OnDrawGizmos()
        {
            if (!ShowGizmos) return;

            if(targetNode != null)
            {
                Gizmos.color = _targetNodeColor;
                Gizmos.DrawSphere(PositionNormalize(targetNode.transform.position), 0.15f);
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