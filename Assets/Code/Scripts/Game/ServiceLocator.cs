using Agents;
using Managers;
using Player;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class ServiceLocator : Singleton<ServiceLocator>
{
    [Title("Debug")]
    [SerializeField] private bool m_debug = false;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private AgentsManager agentsManager = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private NodeCache nodeCache = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private NodeManager nodeManager = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private DeathManager deathManager = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private PlayerController player = null;

    // DATI SALVATI

    [SerializeField] private bool m_debugLog = false;

    //flag
    private bool hasNotified = false;

    public static event Action OnServiceLoactorCreated;

    protected override void Awake()
    {
        IsPersistent = false;

        base.Awake();
    }

    #region PRIVATE METHODS

    private void CheckCreateProgress()
    {
        if (m_debugLog)
        {
            Debug.LogError("AgentsManager: " + (agentsManager != null));
            Debug.LogError("NodeCache: " + (nodeCache != null));
            Debug.LogError("NodeManager: " + (nodeManager != null));
            Debug.LogError("DeathManager: " + (deathManager != null));
            Debug.LogError("Player: " + (player != null));
        }

        if (agentsManager != null && nodeCache != null && nodeManager != null && deathManager != null && player != null && !hasNotified)
        {
            hasNotified = true;
            OnServiceLoactorCreated?.Invoke();
        }
    }

    #endregion

    #region Getters and Setters

    public AgentsManager AgentsManager
    {
        set
        {
            if (agentsManager == null)
            {
                agentsManager = value;
                CheckCreateProgress();
            }
        }
        get { return agentsManager; }

    }

    public NodeCache NodeCache
    {
        set
        {
            if (nodeCache == null)
            {
                nodeCache = value;
                CheckCreateProgress();
            }
        }
        get { return nodeCache; }
    }

    public DeathManager DeathManager
    {
        set
        {
            if (deathManager == null)
            {
                deathManager = value;
                CheckCreateProgress();
            }
        }
        get { return deathManager; }
    }

    public PlayerController Player
    {
        set
        {
            if (player == null)
            {
                player = value;
                CheckCreateProgress();
            }
        }
        get { return player; }
    }

    public NodeManager NodeManager
    {
        set
        {
            if (nodeManager == null)
            {
                nodeManager = value;
                CheckCreateProgress();
            }
        }
        get { return nodeManager; }
    }

    #endregion
}
