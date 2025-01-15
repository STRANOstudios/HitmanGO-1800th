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
        ShiftManager.OnEnemyTurn += CheckPlayerPresence;
    }

    private void OnDisable()
    {
        ShiftManager.OnEnemyTurn -= CheckPlayerPresence;
    }

    private void CheckPlayerPresence()
    {
        if (m_node.Storages.Any(obj => obj.CompareTag("Player")))
        {
            Exit?.Invoke();
        }
    }
}
