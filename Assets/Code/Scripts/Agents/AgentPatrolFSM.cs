using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Agents
{
    public class AgentPatrolFSM : AgentFSM
    {
        [Title("Patrol")]
        [SerializeField] private Node nodeStart;
        [SerializeField] private Node nodeEnd;

        [FoldoutGroup("Gizmos"), ShowIf("ShowGizmos"), PropertyOrder(10)]
        [SerializeField, ColorPalette] private Color _patrolPathColor = Color.blue;

        private List<Node> patrolPath = new() { };

        private void OnValidate()
        {
            if (nodeStart != null)
            {
                patrolPath.Clear();

                transform.position = nodeStart.transform.position;
                currentNode = nodeStart;

                patrolPath.Add(nodeStart);

                if (nodeEnd != null)
                {
                    // pathfinder

                    patrolPath.Add(nodeEnd);
                }
            }
        }

        #region Gizmos

        private new void OnDrawGizmos()
        {
            if (!ShowGizmos) return;

            if (patrolPath.Count > 0)
            {
                Color _patrolPathColorEnds = Color.Lerp(_patrolPathColor, Color.magenta, 0.5f);

                Gizmos.color = _patrolPathColorEnds;

                Gizmos.DrawSphere(PositionNormalize(patrolPath[0].transform.position), 0.15f);

                if (patrolPath.Count > 1)
                {
                    Gizmos.DrawSphere(PositionNormalize(patrolPath[^1].transform.position), 0.15f);

                    Gizmos.color = _patrolPathColor;

                    for (int i = 0; i < patrolPath.Count - 1; i++)
                    {
                        Gizmos.DrawLine(PositionNormalize(patrolPath[i].transform.position), PositionNormalize(patrolPath[i + 1].transform.position));
                        if (i > 0 && i < patrolPath.Count - 1) Gizmos.DrawSphere(PositionNormalize(patrolPath[i].transform.position), 0.1f);
                    }
                }

            }

            base.OnDrawGizmos();
        }

        #endregion
    }
}
