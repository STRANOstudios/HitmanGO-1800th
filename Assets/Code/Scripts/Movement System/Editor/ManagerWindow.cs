using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    public class ManagerWindow : EditorWindow
    {
        // Show Window Function
        [MenuItem("Tools/Pathline System/Pathline Manager")]
        public static void ShowWindow()
        {
            GetWindow<ManagerWindow>("Pathline Manager");
        }

        private void OnEnable()
        {
            EditorApplication.update += UpdateWindow;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateWindow;
        }

        private void UpdateWindow()
        {
            Repaint();
        }

        public Transform root;
        public PathDesign pathDesign;

        private Vector2 designScrollPosition;

        private bool showDesignSettings = false;
        private bool showLinkSettings = false;
        private bool showNodeSettings = false;
        private bool showDirectionSettings = false;

        private void OnValidate()
        {
            if (pathDesign != null)
            {
                PathComponentModifier.ApplyChanges(pathDesign);
            }
        }

        private void OnGUI()
        {
            SerializedObject obj = new(this);

            GUILayout.Label("Path System Manager", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(obj.FindProperty("root"));

            if (root == null)
            {
                EditorGUILayout.HelpBox("root transform must be set", MessageType.Warning);
            }
            else
            {
                // Draw a vertical box containing the waypoint management buttons.
                EditorGUILayout.BeginVertical("box");
                DrawButtons();
                EditorGUILayout.EndVertical();
            }

            pathDesign = (PathDesign)EditorGUILayout.ObjectField("Path Design", pathDesign, typeof(PathDesign), false);

            if (pathDesign== null)
            {
                EditorGUILayout.HelpBox("Assign a PathDesign asset to modify its settings.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                DrawDesign();
                EditorGUILayout.EndVertical();
            }

            obj.ApplyModifiedProperties();
        }

        #region Node Management

        private void DrawButtons()
        {
            if (GUILayout.Button("Create Node"))
            {
                CreateNode();
            }

            GameObject[] selectedObjects = Selection.gameObjects.Where(obj => obj.GetComponent<Node>() != null).ToArray();

            if (selectedObjects.Length == 1)
            {
                if (GUILayout.Button("Create Lincked Node"))
                {
                    CreateNode(selectedObjects[0].GetComponent<Node>());
                }
                if (GUILayout.Button("Remove Node"))
                {
                    RemoveNode(selectedObjects[0].GetComponent<Node>());
                }
            }
            if (selectedObjects.Length > 1)
            {
                if (GUILayout.Button("Remove Nodes"))
                {
                    RemoveNodes(selectedObjects);
                }
                if (selectedObjects.Length > 2)
                {
                    EditorGUILayout.HelpBox("You can only connect or disconnect two nodes at a time", MessageType.Warning);
                }
                else
                {
                    if (CheckConnectionExistence(selectedObjects))
                    {
                        if (GUILayout.Button("Disconnect Nodes"))
                        {
                            RemoveConnection(selectedObjects);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Connect Nodes"))
                        {
                            CreateConnection(selectedObjects);
                        }
                    }
                }
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Link>() is Link selectedLink)
            {
                if (GUILayout.Button("Remove Link"))
                {
                    RemoveConnection(new[] { selectedLink.NodeFrom.gameObject, selectedLink.NodeTo.gameObject });
                }
            }
        }

        private void CreateConnection(GameObject[] selectedObjects)
        {
            GameObject link = new("Link " + root.childCount);
            link.AddComponent<Link>();
            link.transform.SetParent(root, false);

            Link connection = link.GetComponent<Link>();
            connection.NodeFrom = selectedObjects[0].transform;
            connection.NodeTo = selectedObjects[1].transform;

            Node node = selectedObjects[0].GetComponent<Node>();
            Node node1 = selectedObjects[1].GetComponent<Node>();
            node.neighbours.Add(node1);
            node1.neighbours.Add(node);
        }

        private void RemoveConnection(GameObject[] selectedObjects, bool destroySelf = true)
        {
            // Check if the selected objects are null or destroyed
            if (selectedObjects[0] == null || selectedObjects[1] == null) return;

            // Get nodes from the selected GameObjects
            if (!selectedObjects[0].TryGetComponent(out Node node1) || !selectedObjects[1].TryGetComponent(out Node node2))
                return;

            // Find all connections
            Link linkToRemove = null;
            Link[] allLinks = FindObjectsOfType<Link>();

            foreach (Link link in allLinks)
            {
                // Check if the connection matches either direction
                if ((link.NodeFrom == node1.transform && link.NodeTo == node2.transform) ||
                    (link.NodeFrom == node2.transform && link.NodeTo == node1.transform))
                {
                    linkToRemove = link; // Store the connection to remove
                    break; // Exit loop when connection is found
                }
            }

            // Destroy the connection if found
            if (linkToRemove != null)
            {
                DestroyImmediate(linkToRemove.gameObject);
            }

            // Remove the nodes' connection
            node1.neighbours.Remove(node2);
            if (destroySelf) node2.neighbours.Remove(node1);
        }

        private bool CheckConnectionExistence(GameObject[] selectedObjects)
        {
            // Check if the selected objects are null or destroyed
            if (selectedObjects[0] == null || selectedObjects[1] == null) return false;

            // Get nodes from the selected GameObjects
            if (!selectedObjects[0].TryGetComponent(out Node node1) || !selectedObjects[1].TryGetComponent(out Node node2))
                return false;

            // Find all connections and check if any match
            Link[] allLinks = FindObjectsOfType<Link>();

            foreach (Link link in allLinks)
            {
                // Check if the connection matches either direction
                if ((link.NodeFrom == node1.transform && link.NodeTo == node2.transform) ||
                    (link.NodeFrom == node2.transform && link.NodeTo == node1.transform))
                {
                    return true; // Connection found
                }
            }

            return false; // No connection found
        }

        private void CreateNode(Node selectedNode = null)
        {
            GameObject newNode = new("Node " + root.childCount);
            newNode.AddComponent<Node>();
            newNode.transform.SetParent(root, false);

            Transform position = root;

            // link to selected newNode
            if (selectedNode != null)
            {
                position = selectedNode.transform;

                CreateConnection(new GameObject[] { newNode, selectedNode.gameObject });
            }

            newNode.transform.SetPositionAndRotation(position.position, Quaternion.identity);

            Selection.activeGameObject = newNode;
        }

        private void RemoveNode(Node selectedNode)
        {
            Link[] allLink = FindObjectsOfType<Link>();

            foreach (Link link in allLink)
            {
                if (link.NodeFrom == selectedNode || link.NodeTo == selectedNode)
                {
                    DestroyImmediate(link.gameObject);
                }
            }

            foreach (Node node in selectedNode.neighbours)
            {
                RemoveConnection(new GameObject[] { node.gameObject, selectedNode.gameObject }, false);
            }

            DestroyImmediate(selectedNode.gameObject);
        }

        private void RemoveNodes(GameObject[] selectedObjects)
        {
            foreach (GameObject obj in selectedObjects)
            {
                RemoveNode(obj.GetComponent<Node>());
            }
        }

        #endregion

        #region Design

        private void DrawDesign()
        {
            // draw the scrollview
            designScrollPosition = EditorGUILayout.BeginScrollView(designScrollPosition, GUILayout.Height(400));

            // change the values of the ScriptableObject
            showDesignSettings = EditorGUILayout.Foldout(showDesignSettings, "Design Settings", true);
            if (showDesignSettings)
            {
                pathDesign.yOffset = EditorGUILayout.FloatField("Y Offset", pathDesign.yOffset);
            }

            // Link Settings
            showLinkSettings = EditorGUILayout.Foldout(showLinkSettings, "Link Settings", true);
            if (showLinkSettings)
            {
                pathDesign.Width = EditorGUILayout.FloatField("Width", pathDesign.Width);
                pathDesign.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", pathDesign.StoppingDistance);
                pathDesign.linkColor = EditorGUILayout.ColorField("Link Color", pathDesign.linkColor);
            }

            // Node Settings
            showNodeSettings = EditorGUILayout.Foldout(showNodeSettings, "Node Settings", true);
            if (showNodeSettings)
            {
                pathDesign.spriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.spriteNode, typeof(Sprite), false);
                pathDesign.NodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.NodeScale);
                pathDesign.nodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.nodeColor);
            }

            // Direction Settings
            showDirectionSettings = EditorGUILayout.Foldout(showDirectionSettings, "Direction Settings", true);
            if (showDirectionSettings)
            {
                pathDesign.spriteDirection = (Sprite)EditorGUILayout.ObjectField("Direction Sprite", pathDesign.spriteDirection, typeof(Sprite), false);
                pathDesign.DirectionScale = EditorGUILayout.Vector2Field("Direction Scale", pathDesign.DirectionScale);
                pathDesign.directionColor = EditorGUILayout.ColorField("Direction Color", pathDesign.directionColor);
                pathDesign.DirectionDistance = EditorGUILayout.FloatField("Direction Distance", pathDesign.DirectionDistance);
            }

            // save the changes
            EditorUtility.SetDirty(pathDesign);

            // end the scrollview
            EditorGUILayout.EndScrollView();
        }

        #endregion

    }
}
