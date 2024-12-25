using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    [InitializeOnLoad]
    public class PathComponentModifier
    {
        private static PathDesign localPathDesign = null;

        private static Material sharedMaterial = new(Shader.Find("Sprites/Default"));

        static PathComponentModifier()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            if (localPathDesign != null)
            {
                ApplyChangesToAllNodes(localPathDesign);
                ApplyChangesToAllLinks(localPathDesign);
            }
        }

        private static void ApplyLinkChanges(Link connection, PathDesign pathDesign)
        {
            Vector3 direction = connection.NodeTo.position - connection.NodeFrom.position;
            float distance = direction.magnitude;

            direction.Normalize();

            float stopDistance = Mathf.Min(distance - 2 * pathDesign.StoppingDistance, distance);

            // Calculate the start and end stop positions 
            Vector3 startStopPosition = connection.NodeFrom.position + direction * pathDesign.StoppingDistance;
            Vector3 endStopPosition = connection.NodeTo.position - direction * pathDesign.StoppingDistance;

            if (!connection.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer = connection.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startStopPosition);
            lineRenderer.SetPosition(1, endStopPosition);
            lineRenderer.startWidth = pathDesign.Width;
            lineRenderer.endWidth = pathDesign.Width;
            lineRenderer.startColor = pathDesign.linkColor;
            lineRenderer.endColor = pathDesign.linkColor;
            lineRenderer.material = sharedMaterial;
            lineRenderer.alignment = LineAlignment.TransformZ;

            connection.transform.rotation = Quaternion.LookRotation(Vector3.down);
        }

        private static void ApplyNodeChanges(Node node, PathDesign pathDesign)
        {
            if (!node.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer = node.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = pathDesign.spriteNode;
            spriteRenderer.material = sharedMaterial;
            spriteRenderer.color = pathDesign.nodeColor;
            spriteRenderer.transform.localScale = pathDesign.NodeScale;

            node.transform.SetPositionAndRotation(new(node.transform.position.x, pathDesign.yOffset, node.transform.position.z), Quaternion.AngleAxis(90, Vector3.left));
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
                ApplyNodeChanges(node, pathDesign);
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
                ApplyLinkChanges(link, pathDesign);
            }
        }

        /// <summary>
        /// Applies changes to all nodes and links
        /// </summary>
        /// <param name="pathDesign"></param>
        public static void ApplyChanges(PathDesign pathDesign)
        {
            ApplyChangesToAllNodes(pathDesign);
            ApplyChangesToAllLinks(pathDesign);

            localPathDesign = pathDesign;
        }
    }
}
