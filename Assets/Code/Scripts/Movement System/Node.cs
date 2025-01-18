using System.Collections.Generic;
using UnityEngine;

namespace PathSystem
{
    public class Node : MonoBehaviour
    {
        public List<Node> neighbours = new();
        public float GCost = 0f;
        public float HCost = 0f;
        public Node Parent = null;

        // used for interaction
        public List<GameObject> Storages = new();

        public float FCost => GCost + HCost;
    }
}
