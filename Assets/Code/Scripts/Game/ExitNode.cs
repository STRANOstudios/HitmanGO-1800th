using PathSystem;
using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Node))]
public class ExitNode : MonoBehaviour
{
    private Node m_node;

    public static event Action Exit;

    private void Awake()
    {
        m_node = GetComponent<Node>();
    }

    private void OnEnable()
    {
        GameStatusManager.OnEnemyTurn += CheckPlayerPresence;
    }

    private void OnDisable()
    {
        GameStatusManager.OnEnemyTurn -= CheckPlayerPresence;
    }

    private void CheckPlayerPresence()
    {
        if (m_node.Storages.Contains(ServiceLocator.Instance.Player.gameObject))
        {
            Debug.Log("Exit");
            Exit?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
    }
}
