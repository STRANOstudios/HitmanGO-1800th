using PathSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
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

    private static List<Node> cachedNodes;
    
    private void Awake()
    {
        if (cachedNodes == null || cachedNodes.Count == 0)
        {
            cachedNodes = new List<Node>(FindObjectsOfType<Node>());
        }
    }

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

    #region Getters & Setters

    public List<Node> GetNodes => cachedNodes;

    #endregion

}
