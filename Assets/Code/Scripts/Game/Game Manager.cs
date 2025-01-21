using Agents;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("Event triggered when the player win")]
    [SerializeField] private TriggerEvent _onWinTrigger = new();

    [Tooltip("Event triggered when the player lose")]
    [SerializeField] private TriggerEvent _onLoseTrigger = new();

    [Serializable] public class TriggerEvent : UnityEvent { }

    public static event Action OnStartGame;
    public static event Action OnEndGame;

    public static event Action OnWinCondition;

    private void OnEnable()
    {
        SceneLoader.OnSceneLoadComplete += OnStart;

        // OnWin condition
        ExitNode.Exit += OnWin;
        KillHandler.OnKill += OnWin;

        // OnLose condition
        AgentsManager.OnKillPlayer += OnLose;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoadComplete -= OnStart;

        ExitNode.Exit -= OnWin;
        KillHandler.OnKill -= OnWin;

        AgentsManager.OnKillPlayer -= OnLose;
    }

    private void OnStart()
    {
        OnStartGame?.Invoke();
    }

    public void OnEnd()
    {
        OnEndGame?.Invoke();
    }

    private void OnWin()
    {
        OnWinCondition?.Invoke();
        _onWinTrigger?.Invoke();
        //OnEnd();
    }

    private void OnLose()
    {
        _onLoseTrigger?.Invoke();
        //OnEnd();
    }

}
