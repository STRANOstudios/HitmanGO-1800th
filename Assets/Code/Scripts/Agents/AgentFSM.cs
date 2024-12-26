using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    public class AgentFSM : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] protected Node currentNode;

        [SerializeField] protected float speed = 1.0f;
        [SerializeField] protected float raycastDistance = 1.0f;
        [SerializeField] protected LayerMask raycastMask;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] protected Node targetNode;
        [FoldoutGroup("Debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] protected List<Node> path;

        [SerializeField, ShowIf("_debug")] private bool _drawGizmos = false;

        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField] private float yOffet = 0.5f;
        // destination
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] private Color _targetNodeColor = Color.yellow;
        // pathfinder
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] private Color _pathColor = Color.red;
        // in move
        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos")]
        [SerializeField, ColorPalette] private Color _nextPathColor = Color.magenta;
                
        private bool ShowGizmos => _drawGizmos && _debug;

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

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || !_debug) return;


        }
    }
}