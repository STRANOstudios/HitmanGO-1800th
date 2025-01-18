using Interactables;
using Player;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class GameStatusManager : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private GameStatus _currentTurn = GameStatus.None;

    [Button("Start")]
    private void StartBtn() => BeginPlayerTurn();

    public static event Action OnEnemyTurn;
    public static event Action OnPlayerTurn;

    private void OnEnable()
    {
        SceneLoader.OnSceneLoadComplete += BeginPlayerTurn;

        PlayerController.OnPlayerEndTurn += BeginEnemyTurn;

        MovementManager.OnEndMovement += BeginPlayerTurn;

        Distractor.OnInteractEnd += BeginEnemyTurn;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoadComplete -= BeginPlayerTurn;

        PlayerController.OnPlayerEndTurn -= BeginEnemyTurn;

        MovementManager.OnEndMovement -= BeginPlayerTurn;

        Distractor.OnInteractEnd -= BeginEnemyTurn;
    }

    private void BeginEnemyTurn()
    {
        _currentTurn = GameStatus.Enemy;
        OnEnemyTurn?.Invoke();
    }

    private void BeginPlayerTurn()
    {
        _currentTurn = GameStatus.Player;
        OnPlayerTurn?.Invoke();
    }

    enum GameStatus
    {
        None,
        Player,
        Enemy
    }
}
