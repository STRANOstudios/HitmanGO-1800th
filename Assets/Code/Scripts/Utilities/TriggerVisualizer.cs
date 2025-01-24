using UnityEngine;

public class TriggerVisualizer : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Color _gizmoColor = Color.green;

    private void Start() { }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        // Get the Collider component
        if (!TryGetComponent<Collider>(out var collider)) return;

        Gizmos.color = _gizmoColor;

        // Draw the Collider based on its type
        if (collider is BoxCollider boxCollider)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (collider is SphereCollider sphereCollider)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
        else if (collider is CapsuleCollider capsuleCollider)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            DrawCapsuleGizmos(capsuleCollider);
        }
        else if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Gizmos.DrawMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }

        Gizmos.matrix = Matrix4x4.identity; // Reset matrix
    }

    // Helper to draw a Capsule Collider
    private void DrawCapsuleGizmos(CapsuleCollider capsule)
    {
        Vector3 center = capsule.center;
        float radius = capsule.radius;
        float height = Mathf.Max(0, capsule.height / 2 - radius);

        // Draw main capsule body
        Gizmos.DrawWireSphere(center + Vector3.up * height, radius);
        Gizmos.DrawWireSphere(center - Vector3.up * height, radius);
        Gizmos.DrawLine(center + Vector3.up * height + Vector3.right * radius, center - Vector3.up * height + Vector3.right * radius);
        Gizmos.DrawLine(center + Vector3.up * height - Vector3.right * radius, center - Vector3.up * height - Vector3.right * radius);
        Gizmos.DrawLine(center + Vector3.up * height + Vector3.forward * radius, center - Vector3.up * height + Vector3.forward * radius);
        Gizmos.DrawLine(center + Vector3.up * height - Vector3.forward * radius, center - Vector3.up * height - Vector3.forward * radius);
    }
}
