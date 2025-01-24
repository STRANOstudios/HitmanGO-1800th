using Agents;
using PathSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
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
    [Button]
    public void Clear()
    {
        nodes.Clear();
    }
#if UNITY_EDITOR
    [Button]
    public void Save()
    {
        foreach (Node node in FindObjectsOfType<Node>())
        {
            EditorUtility.SetDirty(node);
            AssetDatabase.SaveAssets();
        }
    }
#endif

    // Public property to access the nodes list (read-only)
    public static List<Node> Nodes => nodes;

    private void Awake()
    {
        RefreshNodes();
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
