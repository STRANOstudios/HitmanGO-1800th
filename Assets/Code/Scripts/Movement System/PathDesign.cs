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
        public float _width = 0.1f;
        public float _stoppingDistance = 0f;
        public Color linkColor = Color.black;

        [Header("Node Settings")]
        public Sprite spriteNode = null;
        public Vector2 _nodeScale = Vector2.one;
        public Color nodeColor = Color.black;

        [Header("Exit Node Settings")]
        public Sprite exitSpriteNode = null;
        public Vector2 _exitNodeScale = Vector2.one;
        public Color exitNodeColor = Color.black;

        [Header("Player Indicator Settings")]
        public GameObject playerIndicator = null;
        public float _playerIndicatorDistance = 0f;

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

        public float PlayerIndicatorDistance
        {
            get { return _playerIndicatorDistance; }
            set { _playerIndicatorDistance = Mathf.Max(value, 0f); }
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

        public Vector2 ExitNodeScale
        {
            get { return _exitNodeScale; }
            set
            {
                // Applica la validazione manuale
                _exitNodeScale.x = Mathf.Max(value.x, 0f);
                _exitNodeScale.y = Mathf.Max(value.y, 0f);
            }
        }
    }
}

