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

        public float FCost => GCost + HCost;
    }
}
