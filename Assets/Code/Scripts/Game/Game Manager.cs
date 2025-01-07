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

    private void OnEnable()
    {
        // Win condition
        ExitNode.Exit += Win;
        KillHandler.OnKill += Win;

        // Lose condition
        AgentsManager.OnKillPlayer += Lose;
    }

    private void OnDisable()
    {
        ExitNode.Exit -= Win;
        KillHandler.OnKill -= Win;

        AgentsManager.OnKillPlayer -= Lose;
    }

    // player win
    private void Win()
    {
        _onWinTrigger?.Invoke();
    }

    // player death
    private void Lose()
    {
        _onLoseTrigger?.Invoke();
    }

}
