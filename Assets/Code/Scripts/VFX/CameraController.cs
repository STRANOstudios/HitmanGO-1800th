using Cinemachine;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VFX
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    [InitializeOnLoad]
    public class CameraController : MonoBehaviour
    {
        [Title("Settings")]
        [Tooltip("The target that the camera will look at"), LabelWidth(100)]
        [SerializeField] private Transform lookAt = null;

        [HorizontalGroup("angle"), LabelWidth(100)]
        [SerializeField] private Range valueRange = new Range() { min = -90, max = 90, wrap = false };

        [LabelWidth(100)]
        [SerializeField] private float speed = 1.0f;

        [HorizontalGroup("time"), LabelWidth(100)]
        [SerializeField] private float accelTime = 0.1f;
        [HorizontalGroup("time"), LabelWidth(100)]
        [SerializeField] private float decelTime = 0.1f;

        [HorizontalGroup("input", width: 0.8f), LabelWidth(100)]
        [SerializeField, ReadOnly] private float inputAxisValue = 0f;
        [HorizontalGroup("input", width: 0.2f)]
        [SerializeField] private bool invert = true;

        [SerializeField] private RecenterToTargetHeadling recenterToTargetHeadling;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [FoldoutGroup("debug"), ShowIf("_debug")]
        [SerializeField] private bool _debugLog = false;

        [FoldoutGroup("debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] private float currentRotation = 0f;

        [FoldoutGroup("debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] private float recenterStartTime = 0f;

        [FoldoutGroup("debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly] private bool isRecentring = true;

        [FoldoutGroup("debug"), ShowIf("_debug")]
        [ShowInInspector, ReadOnly, HideLabel, ProgressBar(0, 1f, DrawValueLabel = false)]
        private float progress = 0f;

        [SerializeField] private bool _drawGizmos = true;

        [FoldoutGroup("drawGizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _targetColor = Color.red;

        [FoldoutGroup("drawGizmos"), ShowIf("_drawGizmos")]
        [SerializeField, Range(1f, 10f)] private float _targetSize = 2;

        [FoldoutGroup("drawGizmos"), ShowIf("_drawGizmos")]
        [SerializeField, ColorPalette] private Color _trackColor = Color.green;

        [FoldoutGroup("drawGizmos"), ShowIf("_drawGizmos")]
        [SerializeField, Range(3f, 50f)] private int _trackDetails = 30;

        #region Fileds

        [Serializable]
        [InlineProperty(LabelWidth = 13)]
        public struct Range
        {
            [LabelText("")]
            [HorizontalGroup] public float min;
            [LabelText("to")]
            [HorizontalGroup] public float max;
            [LabelWidth(35)]
            [HorizontalGroup] public bool wrap;
        }

        [Serializable]
        public struct RecenterToTargetHeadling
        {
            public bool enable;
            [Unit(Units.Second, Units.Second)] public float waitTime;
            [Unit(Units.Second, Units.Second)] public float recenteringTime;
        }

        private void CheckInput()
        {
            valueRange.min = Mathf.Clamp(valueRange.min, -180, 0);
            valueRange.max = Mathf.Clamp(valueRange.max, 0, 180);

            speed = speed < 0 ? speed = 0 : speed;

            accelTime = accelTime < 0 ? accelTime = 0 : accelTime;
            decelTime = decelTime < 0 ? decelTime = 0 : decelTime;

            recenterToTargetHeadling.waitTime = recenterToTargetHeadling.waitTime < 0 ? recenterToTargetHeadling.waitTime = 0 : recenterToTargetHeadling.waitTime;
            recenterToTargetHeadling.recenteringTime = recenterToTargetHeadling.recenteringTime < 0 ? recenterToTargetHeadling.recenteringTime = 0 : recenterToTargetHeadling.recenteringTime;
        }

        #endregion

        private float rotationValue;

        private void OnValidate()
        {
            CheckInput();

            CinemachineVirtualCamera virtualCam = GetComponent<CinemachineVirtualCamera>();

            if (lookAt == null || lookAt != virtualCam.LookAt)
            {
                virtualCam.LookAt = lookAt;
            }
        }

        private void Start()
        {
            currentRotation = transform.eulerAngles.y;
            recenterStartTime = Time.time;
        }

        private void FixedUpdate()
        {
            EditorUtility.SetDirty(this);

            HandleMouseRotation();
            HandleTouchRotation();

            if (recenterToTargetHeadling.enable && !isRecentring && Time.time > recenterStartTime + recenterToTargetHeadling.waitTime)
            {
                if (_debugLog) Debug.Log("Recentering");

                HandleRecenterToTargetHeading();
            }
        }

        private void HandleMouseRotation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_debugLog) Debug.Log("Mouse Clicked");
                StartInteraction();
            }
            if (Input.GetMouseButtonUp(0))
            {
                EndInteraction();
            }
            else if (Input.GetMouseButton(0))
            {
                Interaction(Input.GetAxis("Mouse X"));
            }
        }

        private void HandleTouchRotation()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (_debugLog) Debug.Log("Touching");
                        StartInteraction();
                        break;

                    case TouchPhase.Moved:
                        Interaction(touch.deltaPosition.x);
                        break;

                    case TouchPhase.Ended:
                        EndInteraction();
                        break;
                }
            }
        }

        private void HandleRecenterToTargetHeading()
        {
            if (progress == 0f)
            {
                rotationValue = currentRotation;
            }

            float recenterElapsedTime = Time.time - (recenterStartTime + recenterToTargetHeadling.waitTime);
            progress = Mathf.Clamp01(recenterElapsedTime / recenterToTargetHeadling.recenteringTime);

            currentRotation = Mathf.Lerp(rotationValue, 0, progress);

            transform.position = GetArcPosition(currentRotation);

            if (progress >= 1f)
            {
                progress = 0f;
                isRecentring = true;
            }
        }

        #region Private Methods

        private float WrapAngle(float angle)
        {
            if (_debugLog) Debug.Log("Wrapping");

            if (angle < valueRange.min)
            {
                angle = valueRange.max;
            }
            else if (angle > valueRange.max)
            {
                angle = valueRange.min;
            }

            return angle;
        }

        private Vector3 GetArcPosition(float angle)
        {
            angle += -90;

            Vector3 pos = lookAt.position;
            pos.y = transform.position.y;

            float radius = Vector3.Distance(pos, transform.position);
            Vector3 offset = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angle) * radius);

            return offset;
        }

        private void StartInteraction()
        {
            progress = 0f;
            isRecentring = true;
        }

        private void Interaction(float delta)
        {
            if (invert) delta *= -1f;
            currentRotation += delta * speed;

            inputAxisValue = delta;

            if (valueRange.wrap)
                currentRotation = WrapAngle(currentRotation);
            else
                currentRotation = Mathf.Clamp(currentRotation, valueRange.min, valueRange.max);

            rotationValue = currentRotation;
            transform.position = GetArcPosition(currentRotation);
        }

        private void EndInteraction()
        {
            inputAxisValue = 0;
            progress = 0f;
            isRecentring = false;
            recenterStartTime = Time.time;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || !enabled) return;

            if (lookAt != null)
            {
                Gizmos.color = _targetColor;
                Gizmos.DrawSphere(lookAt.position, _targetSize);

                Gizmos.color = _trackColor;

                Vector3 center = lookAt.position;
                center.y = transform.position.y;

                float rotationRadius = Vector3.Distance(center, transform.position);

                DrawArc(center, rotationRadius, valueRange.min * -1, valueRange.max * -1, _trackDetails);
            }
        }

        private void DrawArc(Vector3 center, float radius, float minAngle, float maxAngle, int segments)
        {
            float angleStep = (maxAngle - minAngle) / segments;
            Vector3 previousPoint = center + Quaternion.Euler(0, minAngle, 0) * Vector3.back * radius;

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = minAngle + angleStep * i;
                Vector3 currentPoint = center + Quaternion.Euler(0, currentAngle, 0) * Vector3.back * radius;

                Gizmos.DrawLine(previousPoint, currentPoint);

                previousPoint = currentPoint;
            }
        }

        #endregion
    }
}
