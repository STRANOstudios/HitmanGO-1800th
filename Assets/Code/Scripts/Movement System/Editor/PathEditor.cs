using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    [InitializeOnLoad]
    public class PathEditor
    {
        // Gizmos Settings
        private static Color _selectedColor = Color.yellow;
        private static Color _unselectedColor = Color.yellow * 0.5f;
        private static Color _selectedErrorColor = Color.red;
        private static Color _unselectedErrorColor = Color.red * 0.5f;
        private static Color _blueSelectedColor = Color.blue;
        private static Color _blueUnselectedColor = Color.blue * 0.5f;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawSceneGizmo(Link link, GizmoType gizmoType)
        {
            Gizmos.color = (gizmoType & GizmoType.Selected) != 0 ? _selectedColor : _unselectedColor;

            if (link.NodeTo == null || link.NodeFrom == null)
            {
                Gizmos.color = (gizmoType & GizmoType.Selected) != 0 ? _selectedErrorColor : _unselectedErrorColor;
                Gizmos.DrawSphere(link.transform.position, 0.1f);

                return;
            }

            link.transform.position = Vector3.Lerp(link.NodeFrom.position, link.NodeTo.position, 0.5f);
            Gizmos.DrawSphere(link.transform.position, 0.1f);
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawScene(Node node, GizmoType gizmoType)
        {
            Gizmos.color = (gizmoType & GizmoType.Selected) != 0 ? _blueSelectedColor : _blueUnselectedColor;
            Gizmos.DrawSphere(node.transform.position, 0.1f);
        }
    }
}
