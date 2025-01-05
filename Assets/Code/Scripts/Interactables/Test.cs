using Agents;
using PathSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactables
{
    public class Test : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField, Required] private Node _node;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Vector3 _position = Vector3.zero;
        [SerializeField] private Vector3 _scale = Vector3.one;

        [SerializeField] private bool setTarget = false;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [SerializeField] private List<Agent> agents = new();
        [SerializeField] private bool _drawGizmos = false;
        [SerializeField, ColorPalette] private Color _positionColor = Color.yellow;
        [SerializeField, ColorPalette] private Color _boxColor = Color.red;

        [Button]
        public void ActiveButton()
        {
            agents.Clear();

            Collider[] colliders = Physics.OverlapBox(transform.position + _position, _scale / 2, Quaternion.identity, _layerMask);

            foreach (Collider collider in colliders)
            {
                if (_debug) Debug.Log(collider.name);

                agents.Add(collider.GetComponent<Agent>());
            }

            if (setTarget)
            {
                AgentsManager.Instance.NewTarget(_node, agents);
            }
        }

        private void OnValidate()
        {
            if (_node != null)
            {
                transform.position = _node.transform.position;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            Gizmos.color = _positionColor;
            Gizmos.DrawSphere(transform.position + Vector3.up, 0.5f);

            Gizmos.color = _boxColor;
            Gizmos.DrawWireCube(transform.position + _position, _scale);
        }
    }
}
