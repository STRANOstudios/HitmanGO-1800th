using System.Collections.Generic;
using UnityEngine;

namespace PathSystem
{
    public class Node : MonoBehaviour
    {
        public List<Node> neighbours = new();
    }
}
