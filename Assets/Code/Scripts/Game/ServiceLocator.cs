using Agents;
using DataSystem;
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
    [ShowInInspector, ReadOnly] private MovementManager nodeManager = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private DeathManager deathManager = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private PlayerController player = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private StyleManager pathDesign = null;

    [ShowIfGroup("m_debug")]
    [ShowInInspector, ReadOnly] private AchievementManager achievementManager = null;

    // DATI SALVATI

    [SerializeField] private bool m_debugLog = false;

    //flag
    private bool hasNotified = false;

    public static event Action OnServiceLocatorCreated;

    protected override void Awake()
    {
        IsPersistent = false;

        base.Awake();
    }

    #region PRIVATE METHODS

    private void CheckCreateProgress()
    {
        //if (m_debugLog)
        //{
        //    if (agentsManager == null) Debug.LogError("Missing reference AgentsManager");
        //    if (nodeCache == null) Debug.LogError("Missing reference NodeCache");
        //    if (nodeManager == null) Debug.LogError("Missing reference NodeManager");
        //    if (deathManager == null) Debug.LogError("Missing reference DeathManager");
        //    if (player == null) Debug.LogError("Missing reference Player");
        //}

        if (agentsManager != null && nodeCache != null && nodeManager != null && deathManager != null && player != null && pathDesign != null && achievementManager != null && !hasNotified)
        {
            hasNotified = true;
            OnServiceLocatorCreated?.Invoke();
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

    public MovementManager NodeManager
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

    public StyleManager PathDesign
    {
        set
        {
            if (pathDesign == null)
            {
                pathDesign = value;
                CheckCreateProgress();
            }
        }
        get { return pathDesign; }
    }

    public AchievementManager AchievementManager
    {
        set
        {
            if (achievementManager == null)
            {
                achievementManager = value;
                CheckCreateProgress();
            }
        }
        get { return achievementManager; }
    }

    #endregion
}
