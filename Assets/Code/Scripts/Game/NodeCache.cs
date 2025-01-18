using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NodeCache: manages nodes and is executable only in the Editor.
/// Stores node data for internal use and lookup purposes.
/// </summary>
public class NodeCache : MonoBehaviour
{
    // Static list to store nodes
    [ShowInInspector, ReadOnly]
    private static List<Node> nodes = new();

    [Button]
    public void Refresh()
    {
        RefreshNodes();
    }

    // Public property to access the nodes list (read-only)
    public static IReadOnlyList<Node> Nodes => nodes;

    private void Awake()
    {
        ServiceLocator.Instance.NodeCache = this;
    }

    /// <summary>
    /// Collects and stores all nodes present in the scene.
    /// </summary>
    private void RefreshNodes()
    {
        // Find all Node components in the current scene
        nodes = new List<Node>(FindObjectsOfType<Node>());

        // Log the result for debugging purposes (optional)
        Debug.Log($"NodeCache updated: {nodes.Count} nodes found.");
    }
}
