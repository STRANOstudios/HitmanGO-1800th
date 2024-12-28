using Agents;
using PathSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class AgentsTester : MonoBehaviour
{
    public Node TargetNode;

    public List<AgentFSM> agents = new();

    void Start()
    {
        OnInitialized();
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnInitialized;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnInitialized;
    }

    private void OnValidate()
    {
        if(TargetNode != null)
        {
            SetTarget();
        }
    }

    private void OnInitialized()
    {
        agents.Clear();
        agents.AddRange(FindObjectsOfType<AgentFSM>().ToList());
    }

    private void SetTarget()
    {
        Debug.Log("Set Target");
        foreach (AgentFSM agent in agents)
        {
            agent.SetTargetNode = TargetNode;
        }
    }
}
