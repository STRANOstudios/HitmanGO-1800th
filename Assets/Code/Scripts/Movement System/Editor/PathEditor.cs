using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    [InitializeOnLoad]
    public class PathEditor
    {
        // Parametri globali per la gestione delle linee e nodi
        private static float yOffset = 1f;
        private static Color rowColor = Color.black;
        private static float rowWidth = 0.1f;
        private static float stoppingDistance = 1f;
        private static Sprite spriteNode = null;
        private static Color nodeColor = Color.white;
        private static Vector2 nodeScale = Vector2.one;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawSceneGizmo(Connection connection, GizmoType gizmoType)
        {
            if(connection.NodeTo == null || connection.NodeFrom == null) return;

            Vector3 direction = connection.NodeTo.position - connection.NodeFrom.position;
            float distance = direction.magnitude;

            // Normalizza la direzione per avere una direzione unitari
            direction.Normalize();

            // Riduci la distanza per fermarsi prima dei nodi
            float stopDistance = Mathf.Min(distance - 2 * stoppingDistance, distance);

            // Calcola la posizione di fermata per entrambi i nodi
            Vector3 startStopPosition = connection.NodeFrom.position + direction * stoppingDistance;
            Vector3 endStopPosition = connection.NodeTo.position - direction * stoppingDistance;

            if(!connection.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer = connection.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startStopPosition);
            lineRenderer.SetPosition(1, endStopPosition);
            lineRenderer.startWidth = rowWidth;
            lineRenderer.endWidth = rowWidth;
            lineRenderer.startColor = rowColor;
            lineRenderer.endColor = rowColor;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.alignment = LineAlignment.TransformZ;

            connection.transform.rotation = Quaternion.LookRotation(Vector3.down);
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawScene(Node node, GizmoType gizmoType)
        {
            if(!node.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer = node.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = spriteNode;
            spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
            spriteRenderer.color = nodeColor;
            spriteRenderer.transform.localScale = new(nodeScale.x, 1f, nodeScale.y);

            node.transform.SetPositionAndRotation(new() { x = node.transform.position.x, y = yOffset, z = node.transform.position.z }, Quaternion.LookRotation(Vector3.down));
        }
    }
}
