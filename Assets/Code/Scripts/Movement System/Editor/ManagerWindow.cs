using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PathSystem
{
    public class ManagerWindow : OdinEditorWindow
    {
        // Path data to store the settings
        private PathData path;

        // Flag per verificare se i dati sono stati caricati
        private bool isInitialized = false;

        // Show Window Function
        [MenuItem("Tools/Pathline System/Pathline Manager")]
        public static void ShowWindow()
        {
            GetWindow<ManagerWindow>("Pathline Manager");
        }

        // Metodo per inizializzare e caricare i dati
        private new void Initialize()
        {
            Debug.Log("Initialize");


            if (isInitialized) return;

            path = SaveSystem.Load<PathData>("Settings") ?? new PathData();
            LoadSettingsFromPathData();
            isInitialized = true;
        }

        // General Settings
        [BoxGroup("General Settings")]
        [LabelText("Y-Axis Offset")]
        [Tooltip("Vertical offset (on the Y-axis) applied to the pathline.")]
        public float YOffset;

        // Line Settings
        [BoxGroup("Line Settings")]
        [LabelText("Row Color")]
        [Tooltip("Color of the pathline rows")]
        public Color RowColor;

        [BoxGroup("Line Settings")]
        [LabelText("Row Width"), MinValue(0f)]
        [Tooltip("Width of the pathline rows")]
        public float RowWidth;

        [BoxGroup("Line Settings")]
        [LabelText("Stopping Distance"), MinValue(0f)]
        [Tooltip("Minimum distance to stop before reaching a node.")]
        public float StoppingDistance;

        // Nodes Settings
        [BoxGroup("Nodes Settings")]
        [LabelText("Node Sprite")]
        [Tooltip("The graphical sprite that represents the nodes visible in the game")]
        public Sprite SpriteNode;

        [BoxGroup("Nodes Settings")]
        [LabelText("Node Color")]
        [Tooltip("Color of the pathline nodes")]
        public Color PointColor;

        [BoxGroup("Nodes Settings")]
        [LabelText("Node Scale"), MinValue(0f)]
        [Tooltip("Scale of the pathline nodes")]
        public Vector2 PointScale;

        // Direction Indicator Settings
        [BoxGroup("Direction Indicator Settings")]
        [LabelText("Indicator Sprite")]
        [Tooltip("The graphical sprite that represents the direction indicator visible in the game")]
        public Sprite IndicatorSprite;

        [BoxGroup("Direction Indicator Settings")]
        [LabelText("Color")]
        [Tooltip("Color of the pathline direction")]
        public Color DirectionColor;

        [BoxGroup("Direction Indicator Settings")]
        [LabelText("Scale"), MinValue(0f)]
        [Tooltip("Scale of the pathline direction")]
        public Vector2 DirectionScale;

        [BoxGroup("Direction Indicator Settings")]
        [LabelText("Rotation Offset"), Range(0f, 360f)]
        [Tooltip("Angular rotation offset for the direction indicator around the node center.")]
        public float RotationOffset;

        private  void OnValidated()
        {
            Debug.Log("saveing");
            // Inizializza i dati alla prima chiamata di OnGUI
            Initialize();

            // Controlla se ci sono modifiche per salvare i dati
            if (GUI.changed)
            {
                SaveSettingsToPathData();
                SaveSystem.Save(path, "Settings");
            }
        }

        // Save settings from fields to the PathData instance
        private void SaveSettingsToPathData()
        {
            if (path == null) path = new PathData();

            path.YOffset = YOffset;
            path.RowColor = RowColor;
            path.RowWidth = RowWidth;
            path.StoppingDistance = StoppingDistance;
            path.SpriteNode = SpriteNode;
            path.PointColor = PointColor;
            path.PointScale = PointScale;
            path.IndicatorSprite = IndicatorSprite;
            path.DirectionColor = DirectionColor;
            path.DirectionScale = DirectionScale;
            path.RotationOffset = RotationOffset;
        }

        // Load settings from the PathData instance to fields
        private void LoadSettingsFromPathData()
        {
            if (path == null) return;

            YOffset = path.YOffset;
            RowColor = path.RowColor;
            RowWidth = path.RowWidth;
            StoppingDistance = path.StoppingDistance;
            SpriteNode = path.SpriteNode;
            PointColor = path.PointColor;
            PointScale = path.PointScale;
            IndicatorSprite = path.IndicatorSprite;
            DirectionColor = path.DirectionColor;
            DirectionScale = path.DirectionScale;
            RotationOffset = path.RotationOffset;
        }
    }

    [System.Serializable]
    public class PathData
    {
        public float YOffset;
        public Color RowColor;
        public float RowWidth;
        public float StoppingDistance;
        public Sprite SpriteNode;
        public Color PointColor;
        public Vector2 PointScale;
        public Sprite IndicatorSprite;
        public Color DirectionColor;
        public Vector2 DirectionScale;
        public float RotationOffset;
    }
}
