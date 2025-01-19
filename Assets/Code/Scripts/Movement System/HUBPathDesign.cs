using UnityEngine;

namespace PathSystem
{
    [CreateAssetMenu(fileName = "PathDesign", menuName = "PathSystem/HUB PathDesign", order = 0)]
    [System.Serializable]
    public class HUBPathDesign : PathDesign
    {
        [Header("Unlock Link Settings")]
        public float _unlockWidth = 0.1f;
        public float _unlockStoppingDistance = 0f;
        public Color unlockLinkColor = Color.black;

        [Header("Unlock Node Settings")]
        public Sprite unlockSpriteNode = null;
        public Vector2 _unlockNodeScale = Vector2.one;
        public Color unlockNodeColor = Color.black;

        public float UnlockWidth
        {
            get { return _unlockWidth; }
            set { _unlockWidth = Mathf.Max(value, 0f); }
        }

        public float UnlockStoppingDistance
        {
            get { return _unlockStoppingDistance; }
            set { _unlockStoppingDistance = Mathf.Max(value, 0f); }
        }

        public Vector2 UnlockNodeScale
        {
            get { return _unlockNodeScale; }
            set
            {
                _unlockNodeScale.x = Mathf.Max(value.x, 0f);
                _unlockNodeScale.y = Mathf.Max(value.y, 0f);
            }
        }
    }
}
