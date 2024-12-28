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
            EditorApplication.hierarchyChanged += InizializeScene;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateWindow;
            EditorApplication.hierarchyChanged -= InizializeScene;
        }

        private void UpdateWindow()
        {
            Repaint();
            OnValidate();
        }

        private void InizializeScene()
        {
            pathDesign = FindAnyObjectByType<StyleManager>().PathDesign;
            exitNode = FindAnyObjectByType<TriggerCustomHandler>().gameObject;
        }

        public Transform root;
        public PathDesign pathDesign;

        private Vector2 designScrollPosition;

        private GameObject exitNode = null;

        private bool showDesignSettings = false;
        private bool showLinkSettings = false;
        private bool showNodeSettings = false;
        private bool showExitNodeSettings = false;
        private bool showUnlockLinkSettings = false;
        private bool showUnlockNodeSettings = false;

        private void OnValidate()
        {
            if (pathDesign != null)
            {
                PathComponentModifier.ApplyChanges(pathDesign, exitNode);
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

            #region Path Design Management

            //pathDesign = (PathDesign)EditorGUILayout.ObjectField("Path Design", pathDesign, typeof(PathDesign), false);
            InizializeScene();

            if (pathDesign == null)
            {
                EditorGUILayout.HelpBox("Make a Style Manager in the scene", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");

                // draw the scrollview
                designScrollPosition = EditorGUILayout.BeginScrollView(designScrollPosition, GUILayout.ExpandHeight(true));

                DrawDesign();
                if (pathDesign as HUBPathDesign)
                {
                    DrawHUBDesign();
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();
            }

            #endregion

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
                if (pathDesign as HUBPathDesign == null)
                {
                    if (exitNode == null)
                    {
                        if (GUILayout.Button("Set Node to Exit Node"))
                        {
                            SetExitNode(selectedObjects[0]);
                        }
                    }
                    else if (selectedObjects[0] == exitNode)
                    {
                        if (GUILayout.Button("Remove Exit Node"))
                        {
                            RemoveExitNode();
                        }
                    }
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

            if (selectedNode.gameObject == exitNode)
            {
                exitNode = null;
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

        private void SetExitNode(GameObject selectedObject)
        {
            if (exitNode != null) return;

            exitNode = selectedObject;

            selectedObject.AddComponent<BoxCollider>();
            selectedObject.GetComponent<BoxCollider>().isTrigger = true;

            selectedObject.AddComponent<TriggerCustomHandler>();

            OnValidate();
        }

        private void RemoveExitNode()
        {
            if (exitNode != null)
            {
                DestroyImmediate(exitNode.GetComponent<TriggerCustomHandler>());
                DestroyImmediate(exitNode.GetComponent<BoxCollider>());
                DestroyImmediate(exitNode.GetComponent<Rigidbody>());
                exitNode = null;

                OnValidate();
            }
        }

        #endregion

        #region Design

        private void DrawDesign()
        {
            // change the values of the ScriptableObject
            showDesignSettings = EditorGUILayout.Foldout(showDesignSettings, "Design Settings", true);
            if (showDesignSettings)
            {
                pathDesign.yOffset = EditorGUILayout.FloatField("Y Offset", pathDesign.yOffset);
            }

            // Link Settings
            showLinkSettings = EditorGUILayout.Foldout(showLinkSettings, pathDesign as HUBPathDesign != null ? "Lock Link Settings" : "Link Settings", true);
            if (showLinkSettings)
            {
                pathDesign.Width = EditorGUILayout.FloatField("Width", pathDesign.Width);
                pathDesign.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", pathDesign.StoppingDistance);
                pathDesign.linkColor = EditorGUILayout.ColorField("Link Color", pathDesign.linkColor);
            }

            // Node Settings
            showNodeSettings = EditorGUILayout.Foldout(showNodeSettings, pathDesign as HUBPathDesign != null ? "Lock Node Settings" : "Node Settings", true);
            if (showNodeSettings)
            {
                pathDesign.spriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.spriteNode, typeof(Sprite), false);
                pathDesign.NodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.NodeScale);
                pathDesign.nodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.nodeColor);
            }

            if (pathDesign as HUBPathDesign == null)
            {
                // Node Settings
                showExitNodeSettings = EditorGUILayout.Foldout(showExitNodeSettings, "Exit Node Settings", true);
                if (showExitNodeSettings)
                {
                    pathDesign.exitSpriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.exitSpriteNode, typeof(Sprite), false);
                    pathDesign.ExitNodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.ExitNodeScale);
                    pathDesign.exitNodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.exitNodeColor);
                }
            }

            // save the changes
            EditorUtility.SetDirty(pathDesign);
        }

        private void DrawHUBDesign()
        {
            HUBPathDesign pathDesign = (HUBPathDesign)this.pathDesign;

            // Link Settings
            showUnlockLinkSettings = EditorGUILayout.Foldout(showUnlockLinkSettings, "Unlock Link Settings", true);
            if (showUnlockLinkSettings)
            {
                pathDesign.UnlockWidth = EditorGUILayout.FloatField("Width", pathDesign.UnlockWidth);
                pathDesign.UnlockStoppingDistance = EditorGUILayout.FloatField("Stopping Distance", pathDesign.UnlockStoppingDistance);
                pathDesign.unlockLinkColor = EditorGUILayout.ColorField("Link Color", pathDesign.unlockLinkColor);
            }

            // Node Settings
            showUnlockNodeSettings = EditorGUILayout.Foldout(showUnlockNodeSettings, "Unlock Node Settings", true);
            if (showUnlockNodeSettings)
            {
                pathDesign.unlockSpriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.unlockSpriteNode, typeof(Sprite), false);
                pathDesign.UnlockNodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.UnlockNodeScale);
                pathDesign.unlockNodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.unlockNodeColor);
            }

            // save the changes
            EditorUtility.SetDirty(pathDesign);
        }

        #endregion
    }
}
