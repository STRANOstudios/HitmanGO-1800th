using UnityEngine;

namespace PathSystem
{
    [CreateAssetMenu(fileName = "PathDesign", menuName = "PathSystem/PathDesign", order = 0)]
    [System.Serializable]
    public class PathDesign : ScriptableObject
    {
        [Header("Path Design Settings")]
        public float yOffset = 0f;

        [Header("Link Settings")]
        private float _width = 0.1f;
        private float _stoppingDistance = 0f;
        public Color linkColor = Color.black;

        [Header("Node Settings")]
        public Sprite spriteNode = null;
        private Vector2 _nodeScale = Vector2.one;
        public Color nodeColor = Color.black;

        [Header("Direction Settings")]
        public Sprite spriteDirection = null;
        private Vector2 _directionScale = Vector2.one;
        public Color directionColor = Color.black;
        private float _directionDistance = 1f;

        public float Width
        {
            get { return _width; }
            set { _width = Mathf.Max(value, 0f); }
        }

        public float StoppingDistance
        {
            get { return _stoppingDistance; }
            set { _stoppingDistance = Mathf.Max(value, 0f); }
        }

        public float DirectionDistance
        {
            get { return _directionDistance; }
            set { _directionDistance = Mathf.Max(value, 0f); }
        }

        public Vector2 DirectionScale
        {
            get { return _directionScale; }
            set
            {
                // Applica la validazione manuale
                _directionScale.x = Mathf.Max(value.x, 0f);
                _directionScale.y = Mathf.Max(value.y, 0f);
            }
        }

        public Vector2 NodeScale
        {
            get { return _nodeScale; }
            set
            {
                // Applica la validazione manuale
                _nodeScale.x = Mathf.Max(value.x, 0f);
                _nodeScale.y = Mathf.Max(value.y, 0f);
            }
        }
    }
}

