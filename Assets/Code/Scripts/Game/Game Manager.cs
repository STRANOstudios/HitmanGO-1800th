using Agents;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("Event triggered when the game start after dark transition")]
    [SerializeField] private TriggerEvent _onStart = new();

    [Tooltip("Event triggered when the game end")]
    [SerializeField] private TriggerEvent _onEnd = new();

    [Tooltip("Event triggered when the player win")]
    [SerializeField] private TriggerEvent _onWinTrigger = new();

    [Tooltip("Event triggered when the player lose")]
    [SerializeField] private TriggerEvent _onLoseTrigger = new();

    [Serializable] public class TriggerEvent : UnityEvent { }

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
        _onStart?.Invoke();
    }

    private void OnEnd()
    {
        _onEnd?.Invoke();
    }

    private void OnWin()
    {
        _onWinTrigger?.Invoke();
        OnEnd();
    }

    private void OnLose()
    {
        _onLoseTrigger?.Invoke();
        OnEnd();
    }

}
