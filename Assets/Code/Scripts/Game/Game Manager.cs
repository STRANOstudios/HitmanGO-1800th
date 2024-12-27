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

    // flags
    private bool _isPlayerTurn = true;
    private bool _isPlayerAtExit = false;

    private void OnEnable()
    {
        ShiftManager.OnEnemyTurn += WinCheck;
    }

    private void OnDisable()
    {
        ShiftManager.OnEnemyTurn += WinCheck;
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

    #region private methods

    private void WinCheck()
    {
        if(_isPlayerAtExit && _isPlayerTurn)
        {
            Win();
        }
    }

    #endregion

    #region public methods

    public void OnPlayerEnter()
    {
        _isPlayerAtExit = true;
    }

    #endregion

}
