using Agents;
using PathSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class AgentsTester : MonoBehaviour
{
    [Header("Target Settings")]
    public Node TargetNode;
    [SerializeField, Min(0)] private int turnsBeforeChange = 15;         // Number of turns before changing the target.

    [Header("Agent Settings")]
    public bool infinite = true;

    [Header("Debug")]
    [SerializeField] private bool _debug = false;

    private bool _targetReached = false;
    private HashSet<AgentFSM> _agents = new();
    private int _turnCount = 0;               // Tracks the number of turns elapsed.
    private List<Node> possibleTargets = new(); // List of potential target nodes.

    private void Start()
    {
        InitializeAgents();
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += InitializeAgents;
        ShiftManager.OnPlayerTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= InitializeAgents;
        ShiftManager.OnPlayerTurn -= HandlePlayerTurn;
    }

    private void OnValidate()
    {
        // Mark as reached when the TargetNode is set or updated.
        if (TargetNode != null && Application.isPlaying)
            _targetReached = true;
    }

    private void HandlePlayerTurn()
    {
        if (_targetReached)
        {
            AssignTargetToAgents();
            _targetReached = false;
        }

        // Increment turn count and check if the target needs to be changed.
        _turnCount++;
        if (_turnCount >= turnsBeforeChange)
        {
            ChangeTarget();
            turnsBeforeChange = Random.Range(5, 20);
            _turnCount = 0; // Reset the turn count.
        }
    }

    private void InitializeAgents()
    {
        _agents.Clear();
        _agents.UnionWith(FindObjectsOfType<AgentFSM>());

        possibleTargets.Clear();
        possibleTargets.AddRange(FindObjectsOfType<Node>());
    }

    private void AssignTargetToAgents()
    {
        if (_debug) Debug.Log("Assigning Target to Agents");
        foreach (AgentFSM agent in _agents)
        {
            agent.SetTargetNode = TargetNode;
        }
    }

    private void ChangeTarget()
    {
        if (possibleTargets.Count == 0) return; // Ensure there are targets to cycle through.

        // Cycle to the next target in the list.
        TargetNode = possibleTargets[Random.Range(0, possibleTargets.Count)];
        if (_debug) Debug.Log($"Target changed to: {TargetNode.name}");

        // Mark the target as reached to trigger an update.
        _targetReached = true;
    }
}
