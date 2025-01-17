using HUB;
using PathSystem;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HUBManager))]
public class ComponentModifier : MonoBehaviour
{
    private HUBPathDesign hubPathDesign;
    private HUBManager hubManager;

    private Node[] nodes;
    private Link[] links;

    private List<LineRenderer> lineRenderers = new();

    bool isInitialized = false;

    private void Start()
    {
        hubManager = GetComponent<HUBManager>();
        hubPathDesign = StyleManager.Instance.PathDesign as HUBPathDesign;

        nodes = FindObjectsOfType<Node>();
        links = FindObjectsOfType<Link>();

        ApplyChangesToAllNodes();
        ApplyChangesToAllLinks();
    }

    private void Update()
    {
        if (!isInitialized) return;

        for (int i = 0; i < links.Length; i++)
        {
            ApplyPosition(links[i], lineRenderers[i], IsActive(links[i].gameObject) ? hubPathDesign.UnlockStoppingDistance : hubPathDesign.StoppingDistance);
        }
    }

    public void ApplyChanges()
    {
        ApplyChangesToAllNodes();
        ApplyChangesToAllLinks();
    }

    private void ApplyChangesToAllNodes()
    {
        foreach (var node in nodes)
        {
            CheckNodeType(node);
        }
    }

    private void ApplyChangesToAllLinks()
    {
        foreach (var link in links)
        {
            CheckLinkType(link);
        }
    }

    private void CheckNodeType(Node node)
    {
        if (IsActive(node.gameObject))
        {
            ApplyNodeChanges(node, hubPathDesign.unlockSpriteNode, hubPathDesign.unlockNodeColor, hubPathDesign.UnlockNodeScale, hubPathDesign.yOffset);
        }
        else
        {
            ApplyNodeChanges(node, hubPathDesign.spriteNode, hubPathDesign.nodeColor, hubPathDesign.NodeScale, hubPathDesign.yOffset);
        }
    }

    private void CheckLinkType(Link connection)
    {
        if (IsActive(connection.gameObject))
        {
            ApplyLinkChanges(connection, hubPathDesign.unlockLinkColor, hubPathDesign.UnlockWidth, hubPathDesign.UnlockStoppingDistance, hubPathDesign.yOffset);
        }
        else
        {
            ApplyLinkChanges(connection, hubPathDesign.linkColor, hubPathDesign.Width, hubPathDesign.StoppingDistance, hubPathDesign.yOffset);
        }
    }

    private void ApplyNodeChanges(Node node, Sprite sprite, Color color, Vector3 scale, float yOffset)
    {
        if (!node.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer = node.gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        node.transform.SetPositionAndRotation(new Vector3(node.transform.position.x, yOffset, node.transform.position.z), Quaternion.LookRotation(Vector3.down));
    }

    private void ApplyLinkChanges(Link connection, Color color, float width, float stoppingDistance, float yOffset)
    {
        LineRenderer lineRenderer = connection.gameObject.GetComponent<LineRenderer>();

        lineRenderers.Add(lineRenderer);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        ApplyPosition(connection, lineRenderer, stoppingDistance);

        isInitialized = true;
    }

    private void ApplyPosition(Link connection, LineRenderer lineRenderer, float stoppingDistance)
    {
        Vector3 direction = connection.NodeTo.position - connection.NodeFrom.position;
        direction.Normalize();

        Vector3 startStopPosition = connection.NodeFrom.position + direction * stoppingDistance;
        Vector3 endStopPosition = connection.NodeTo.position - direction * stoppingDistance;

        startStopPosition = new Vector3(startStopPosition.x, hubPathDesign.yOffset, startStopPosition.z);
        endStopPosition = new Vector3(endStopPosition.x, hubPathDesign.yOffset, endStopPosition.z);

        lineRenderer.SetPosition(0, startStopPosition);
        lineRenderer.SetPosition(1, endStopPosition);
    }

    private bool IsActive(GameObject gameObject)
    {
        if (hubManager == null) return false;

        return hubManager.ActiveObjects.Contains(gameObject);
    }
}
