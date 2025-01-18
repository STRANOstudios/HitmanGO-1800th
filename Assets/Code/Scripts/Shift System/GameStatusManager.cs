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
        SceneLoader.OnSceneLoadComplete += GameStart;

        PlayerController.OnPlayerEndTurn += BeginEnemyTurn;

        MovementManager.OnEndMovement += BeginPlayerTurn;
        Distractor.OnInteractEnd += BeginPlayerTurn;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoadComplete -= GameStart;

        PlayerController.OnPlayerEndTurn -= BeginEnemyTurn;

        MovementManager.OnEndMovement -= BeginPlayerTurn;
        Distractor.OnInteractEnd -= BeginPlayerTurn;
    }

    private void GameStart()
    {
        BeginPlayerTurn();
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
