using HUB;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    [InitializeOnLoad]
    public class PathComponentModifier
    {
        private static PathDesign localPathDesign = null;

        private static Material sharedMaterial;

        private static HUBManager hubManager;
        private static GameObject exitNode = null;

        static PathComponentModifier()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnHierarchyChanged;
            sharedMaterial = new(Shader.Find("Sprites/Default"));
        }

        private static void OnHierarchyChanged()
        {
            if (localPathDesign != null)
            {
                ApplyChangesToAllNodes(localPathDesign);
                ApplyChangesToAllLinks(localPathDesign);
            }
            if (hubManager == null)
            {
                hubManager = GameObject.FindObjectOfType<HUBManager>();
            }
        }

        private static void CheckLinkType(Link connection, PathDesign pathDesign)
        {
            if (pathDesign is HUBPathDesign hubPathDesign && IsActive(connection.gameObject))
            {
                ApplyLinkChanges(connection, hubPathDesign.unlockLinkColor, hubPathDesign.UnlockWidth, hubPathDesign.UnlockStoppingDistance, hubPathDesign.yOffset);
            }
            else
            {
                ApplyLinkChanges(connection, pathDesign.linkColor, pathDesign.Width, pathDesign.StoppingDistance, pathDesign.yOffset);
            }

        }

        private static void CheckNodeType(Node node, PathDesign pathDesign)
        {
            if (pathDesign is HUBPathDesign hubPathDesign && IsActive(node.gameObject))
            {
                ApplyNodeChanges(node, hubPathDesign.unlockSpriteNode, hubPathDesign.unlockNodeColor, hubPathDesign.UnlockNodeScale, hubPathDesign.yOffset);
            }
            else
            {
                if (exitNode != null && node.gameObject == exitNode)
                {
                    ApplyNodeChanges(node, pathDesign.exitSpriteNode, pathDesign.exitNodeColor, pathDesign.ExitNodeScale, pathDesign.yOffset);
                }
                else
                {
                    ApplyNodeChanges(node, pathDesign.spriteNode, pathDesign.nodeColor, pathDesign.NodeScale, pathDesign.yOffset);
                }
            }
        }

        private static void ApplyNodeChanges(Node node, Sprite sprite, Color color, Vector3 scale, float yOffset)
        {
            if (!node.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer = node.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.material = sharedMaterial;
            spriteRenderer.color = color;
            spriteRenderer.transform.localScale = new(scale.x, scale.y, 1f);
            spriteRenderer.sortingOrder = 2;

            node.transform.SetPositionAndRotation(new(node.transform.position.x, yOffset, node.transform.position.z), Quaternion.LookRotation(Vector3.down));
        }

        private static void ApplyLinkChanges(Link connection, Color color, float width, float stoppingDistance, float yOffset)
        {
            Vector3 direction = connection.NodeTo.position - connection.NodeFrom.position;
            direction.Normalize();

            // Calculate the start and end stop positions 
            Vector3 startStopPosition = connection.NodeFrom.position + direction * stoppingDistance;
            Vector3 endStopPosition = connection.NodeTo.position - direction * stoppingDistance;

            startStopPosition = new(startStopPosition.x, yOffset, startStopPosition.z);
            endStopPosition = new(endStopPosition.x, yOffset, endStopPosition.z);

            if (!connection.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer = connection.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startStopPosition);
            lineRenderer.SetPosition(1, endStopPosition);

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            lineRenderer.material = sharedMaterial;
            lineRenderer.alignment = LineAlignment.TransformZ;

            lineRenderer.sortingOrder = 1;

            connection.transform.rotation = Quaternion.LookRotation(Vector3.down);
        }

        /// <summary>
        /// Applies changes to all nodes
        /// </summary>
        /// <param name="pathDesign"></param>
        public static void ApplyChangesToAllNodes(PathDesign pathDesign)
        {
            Node[] nodes = GameObject.FindObjectsOfType<Node>();
            foreach (var node in nodes)
            {
                CheckNodeType(node, pathDesign);
            }
        }

        /// <summary>
        /// Applies changes to all links
        /// </summary>
        /// <param name="pathDesign"></param>
        public static void ApplyChangesToAllLinks(PathDesign pathDesign)
        {
            Link[] links = GameObject.FindObjectsOfType<Link>();
            foreach (var link in links)
            {
                CheckLinkType(link, pathDesign);
            }
        }

        /// <summary>
        /// Applies changes to all nodes and links
        /// </summary>
        /// <param name="pathDesign"></param>
        public static void ApplyChanges(PathDesign pathDesign, GameObject exit = null)
        {
            exitNode = exit;

            localPathDesign = pathDesign;

            if (hubManager == null)
            {
                hubManager = GameObject.FindObjectOfType<HUBManager>();
            }

            if (sharedMaterial == null)
            {
                sharedMaterial = new(Shader.Find("Sprites/Default"));
            }

            ApplyChangesToAllNodes(pathDesign);
            ApplyChangesToAllLinks(pathDesign);
        }

        private static bool IsActive(GameObject gameObject)
        {
            if (hubManager == null) return false;

            return hubManager.ActiveObjects.Contains(gameObject);
        }
    }
}
