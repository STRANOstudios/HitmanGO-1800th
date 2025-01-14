using Agents;
using Player;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class ShiftManager : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private Turn _currentTurn = Turn.None;

    [Button("Start")]
    private void StartBtn() => BeginPlayerTurn();

    public static event Action OnEnemyTurn;
    public static event Action OnPlayerTurn;

    private void OnEnable()
    {
        SceneLoader.OnSceneLoadComplete += BeginPlayerTurn;
        PlayerController.OnPlayerEndTurn += BeginEnemyTurn;
        AgentsManager.OnAgentsEndMovement += BeginPlayerTurn;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoadComplete -= BeginPlayerTurn;
        PlayerController.OnPlayerEndTurn -= BeginEnemyTurn;
        AgentsManager.OnAgentsEndMovement -= BeginPlayerTurn;
    }

    private void BeginEnemyTurn()
    {
        _currentTurn = Turn.Enemy;
        OnEnemyTurn?.Invoke();
    }

    private void BeginPlayerTurn()
    {
        _currentTurn = Turn.Player;
        OnPlayerTurn?.Invoke();
    }

    enum Turn
    {
        None,
        Player,
        Enemy
    }
}
