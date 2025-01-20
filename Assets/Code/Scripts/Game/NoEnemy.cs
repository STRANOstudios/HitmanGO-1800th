using System;
using UnityEngine;

public class NoEnemy : MonoBehaviour
{
    public static event Action OnNoEnemy;

    private void OnEnable()
    {
        GameStatusManager.OnEnemyTurn += Execute;
    }

    private void OnDisable()
    {
        GameStatusManager.OnEnemyTurn -= Execute;
    }

    private void Execute()
    {
        OnNoEnemy?.Invoke();
    }
}
