using System;
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

        public Transform root;

        private void OnGUI()
        {
            SerializedObject obj = new(this);

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

            obj.ApplyModifiedProperties();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Create Node"))
            {
                CreateNode();
            }

            GameObject[] selectedObjects = Selection.gameObjects.Where(obj => obj.GetComponent<Node>() != null).ToArray();

            if (selectedObjects.Length < 2)
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
        }

        private void CreateConnection(GameObject[] selectedObjects)
        {
            GameObject link = new("Link " + root.childCount);
            link.AddComponent<Connection>();
            link.transform.SetParent(root, false);

            Connection connection = link.GetComponent<Connection>();
            connection.NodeFrom = selectedObjects[0].transform;
            connection.NodeTo = selectedObjects[1].transform;

            Node node = selectedObjects[0].GetComponent<Node>();
            Node node1 = selectedObjects[1].GetComponent<Node>();
            node.neighbours.Add(node1);
            node1.neighbours.Add(node);
        }

        private void RemoveConnection(GameObject[] selectedObjects, bool destroySelt = true)
        {
            selectedObjects[0].TryGetComponent(out Node node1);
            selectedObjects[1].TryGetComponent(out Node node2);

            Connection[] allLinks = FindObjectsOfType<Connection>();

            foreach (Connection link in allLinks)
            {
                if ((link.NodeFrom == node1.transform && link.NodeTo == node2.transform) ||
                    (link.NodeFrom == node2.transform && link.NodeTo == node1.transform))
                {
                    DestroyImmediate(link.gameObject);
                    break;
                }
            }

            node1.neighbours.Remove(node2);
            if (destroySelt) node2.neighbours.Remove(node1);
        }

        private bool CheckConnectionExistence(GameObject[] selectedObjects)
        {
            selectedObjects[0].TryGetComponent(out Node node1);
            selectedObjects[1].TryGetComponent(out Node node2);

            Connection[] allLinks = FindObjectsOfType<Connection>();

            foreach (Connection link in allLinks)
            {
                if ((link.NodeFrom == node1.transform && link.NodeTo == node2.transform) ||
                    (link.NodeFrom == node2.transform && link.NodeTo == node1.transform))
                {
                    return true;
                }
            }

            return false;
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
            Connection[] allLink = FindObjectsOfType<Connection>();

            foreach (Connection link in allLink)
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
    }
}
