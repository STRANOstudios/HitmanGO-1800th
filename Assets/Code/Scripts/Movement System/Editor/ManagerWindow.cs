using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    public class ManagerWindow : EditorWindow
    {
        #region Variables and Initialization

        [MenuItem("Tools/Pathline System/Pathline Manager")]
        public static void ShowWindow() => GetWindow<ManagerWindow>("Pathline Manager");

        public Transform root;
        private PathDesign pathDesign;
        private GameObject exitNode = null;

        private Vector2 designScrollPosition;
        private bool showDesignSettings, showLinkSettings, showNodeSettings, showExitNodeSettings, showUnlockLinkSettings, showUnlockNodeSettings, showPlayerSettings;

        private void OnEnable()
        {
            if(Application.isPlaying) return;

            EditorApplication.update += UpdateWindow;
            EditorApplication.hierarchyChanged += InitializeScene;
        }

        private void OnDisable()
        {
            if (Application.isPlaying) return;

            EditorApplication.update -= UpdateWindow;
            EditorApplication.hierarchyChanged -= InitializeScene;
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            EditorUtility.SetDirty(pathDesign);
        }

        private void UpdateWindow()
        {
            if (Application.isPlaying) return;

            Repaint();
            ValidatePathDesign();
        }

        private void InitializeScene()
        {
            pathDesign = FindAnyObjectByType<StyleManager>()?.PathDesign;
            exitNode = FindAnyObjectByType<ExitNode>()?.gameObject;
        }

        private void ValidatePathDesign()
        {
            if (pathDesign != null)
                PathComponentModifier.ApplyChanges(pathDesign, exitNode);
        }

        #endregion

        private void OnGUI()
        {
            SerializedObject obj = new(this);

            GUILayout.Label("Path System Manager", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(obj.FindProperty("root"));

            if (root != null)
                DrawNodeManagement();
            else
                EditorGUILayout.HelpBox("Root transform must be set", MessageType.Warning);

            DrawPathDesignSection();

            obj.ApplyModifiedProperties();
        }

        #region GUI Methods

        private void DrawNodeManagement()
        {
            EditorGUILayout.BeginVertical("box");
            DrawNodeButtons();
            EditorGUILayout.EndVertical();
        }

        private void DrawPathDesignSection()
        {
            InitializeScene();

            if (pathDesign == null)
            {
                EditorGUILayout.HelpBox("Create a Style Manager in the scene", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical("box");
            designScrollPosition = EditorGUILayout.BeginScrollView(designScrollPosition, GUILayout.ExpandHeight(true));
            DrawPathDesignSettings();
            if (pathDesign is HUBPathDesign) DrawHUBPathDesignSettings();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawNodeButtons()
        {
            if (GUILayout.Button("Create Node")) CreateNode();

            GameObject[] selectedObjects = Selection.gameObjects.Where(obj => obj.GetComponent<Node>() != null).ToArray();

            if (selectedObjects.Length == 1)
            {
                DrawSingleNodeButtons(selectedObjects[0]);
            }
            else if (selectedObjects.Length > 1)
            {
                DrawMultiNodeButtons(selectedObjects);
            }

            if (Selection.activeGameObject?.GetComponent<Link>() is Link selectedLink)
            {
                if (GUILayout.Button("Remove Link"))
                    RemoveConnection(new[] { selectedLink.NodeFrom.gameObject, selectedLink.NodeTo.gameObject });
            }
        }

        private void DrawSingleNodeButtons(GameObject selectedObject)
        {
            Node node = selectedObject.GetComponent<Node>();
            if (GUILayout.Button("Create Linked Node")) CreateNode(node);
            if (GUILayout.Button("Remove Node")) RemoveNode(node);

            if (pathDesign is not HUBPathDesign)
            {
                if (exitNode == null && GUILayout.Button("Set Node as Exit Node"))
                    SetExitNode(selectedObject);

                if (selectedObject == exitNode && GUILayout.Button("Remove Exit Node"))
                    RemoveExitNode();
            }
        }

        private void DrawMultiNodeButtons(GameObject[] selectedObjects)
        {
            if (GUILayout.Button("Remove Nodes"))
                RemoveNodes(selectedObjects);

            if (selectedObjects.Length == 2)
            {
                if (CheckConnectionExistence(selectedObjects))
                {
                    if (GUILayout.Button("Disconnect Nodes"))
                        RemoveConnection(selectedObjects);
                }
                else
                {
                    if (GUILayout.Button("Connect Nodes"))
                        CreateConnection(selectedObjects);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You can only connect or disconnect two nodes at a time", MessageType.Warning);
            }
        }

        private void DrawPathDesignSettings()
        {
            bool isHUBPathDesign = pathDesign is HUBPathDesign;

            showDesignSettings = EditorGUILayout.Foldout(showDesignSettings, "Design Settings", true);
            if (showDesignSettings)
                pathDesign.yOffset = EditorGUILayout.FloatField("Y Offset", pathDesign.yOffset);

            string tmp = isHUBPathDesign ? "Lock" : "";

            showLinkSettings = EditorGUILayout.Foldout(showLinkSettings, tmp + "Link Settings", true);
            if (showLinkSettings)
            {
                pathDesign.Width = EditorGUILayout.FloatField("Width", pathDesign.Width);
                pathDesign.StoppingDistance = EditorGUILayout.FloatField("Stopping Distance", pathDesign.StoppingDistance);
                pathDesign.linkColor = EditorGUILayout.ColorField("Link Color", pathDesign.linkColor);
            }

            showNodeSettings = EditorGUILayout.Foldout(showNodeSettings, tmp + "Node Settings", true);
            if (showNodeSettings)
            {
                pathDesign.spriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.spriteNode, typeof(Sprite), false);
                pathDesign.NodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.NodeScale);
                pathDesign.nodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.nodeColor);
            }

            // if the path design is a HUBPathDesign, don't show other settings
            if (isHUBPathDesign) return;

            showExitNodeSettings = EditorGUILayout.Foldout(showExitNodeSettings, "Exit Node Settings", true);
            if (showExitNodeSettings)
            {
                pathDesign.exitSpriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", pathDesign.exitSpriteNode, typeof(Sprite), false);
                pathDesign.ExitNodeScale = EditorGUILayout.Vector2Field("Node Scale", pathDesign.ExitNodeScale);
                pathDesign.exitNodeColor = EditorGUILayout.ColorField("Node Color", pathDesign.exitNodeColor);
            }

            showPlayerSettings = EditorGUILayout.Foldout(showPlayerSettings, "Player Indicator Settings", true);
            if (showPlayerSettings)
            {
                pathDesign.playerIndicator = (GameObject)EditorGUILayout.ObjectField("Indicator prefab", pathDesign.playerIndicator, typeof(GameObject), false);
                pathDesign.PlayerIndicatorDistance = EditorGUILayout.FloatField("Stopping Distance", pathDesign.PlayerIndicatorDistance);
            }
        }

        private void DrawHUBPathDesignSettings()
        {
            HUBPathDesign hubPathDesign = (HUBPathDesign)pathDesign;

            showUnlockLinkSettings = EditorGUILayout.Foldout(showUnlockLinkSettings, "Unlock Link Settings", true);
            if (showUnlockLinkSettings)
            {
                hubPathDesign.UnlockWidth = EditorGUILayout.FloatField("Width", hubPathDesign.UnlockWidth);
                hubPathDesign.UnlockStoppingDistance = EditorGUILayout.FloatField("Stopping Distance", hubPathDesign.UnlockStoppingDistance);
                hubPathDesign.unlockLinkColor = EditorGUILayout.ColorField("Link Color", hubPathDesign.unlockLinkColor);
            }

            showUnlockNodeSettings = EditorGUILayout.Foldout(showUnlockNodeSettings, "Unlock Node Settings", true);
            if (showUnlockNodeSettings)
            {
                hubPathDesign.unlockSpriteNode = (Sprite)EditorGUILayout.ObjectField("Node Sprite", hubPathDesign.unlockSpriteNode, typeof(Sprite), false);
                hubPathDesign.UnlockNodeScale = EditorGUILayout.Vector2Field("Node Scale", hubPathDesign.UnlockNodeScale);
                hubPathDesign.unlockNodeColor = EditorGUILayout.ColorField("Node Color", hubPathDesign.unlockNodeColor);
            }
        }

        #endregion

        #region Node Methods

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

            Debug.LogWarning(selectedObject.name);

            exitNode = selectedObject;

            selectedObject.AddComponent<ExitNode>();

            ValidatePathDesign();
        }

        private void RemoveExitNode()
        {
            if (exitNode != null)
            {
                DestroyImmediate(exitNode.GetComponent<ExitNode>());
                exitNode = null;

                ValidatePathDesign();
            }
        }

        #endregion

    }
}
