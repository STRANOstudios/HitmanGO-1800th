using Sirenix.OdinInspector;
using UnityEngine;

namespace HUB
{
    public class ViewController : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField, MinValue(0f)] private float range = 0f;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [SerializeField, ShowIf("_debug")] private Transform objectToMove;
        [SerializeField] private bool _showGizmos = false;

        private Camera mainCamera;
        private float objectDepth = 0f;

        private Vector3 offset;
        private bool isDragging = false;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            Mobile();

            Desktop();
        }

        private void Mobile()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchPosition = touch.position;
                touchPosition.z = objectDepth;

                Vector3 worldTouchPosition = mainCamera.ScreenToWorldPoint(touchPosition);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        DragStart(touchPosition, worldTouchPosition);
                        break;

                    case TouchPhase.Moved:
                        Drag(worldTouchPosition);
                        break;

                    case TouchPhase.Ended:
                        DragEnd();
                        break;
                }
            }
        }

        private void Desktop()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = objectDepth;

            Vector3 worldTouchPosition;

            if (Input.GetMouseButtonDown(0))
            {
                worldTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                DragStart(mousePosition, worldTouchPosition);
            }
            else if (Input.GetMouseButton(0))
            {
                worldTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Drag(worldTouchPosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                DragEnd();
            }
        }

        private void DragStart(Vector3 pos, Vector3 worldTouchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(pos);

            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                objectToMove = hit.transform;
                isDragging = true;

                offset = objectToMove.position - worldTouchPosition;
            }
        }

        private void Drag(Vector3 targetPosition)
        {
            if (isDragging && objectToMove != null)
            {
                targetPosition += offset;

                Vector3 pos = Vector3.Lerp(objectToMove.position, targetPosition, speed * Time.deltaTime);

                if (Vector3.Distance(pos, Vector3.zero) <= range)
                {
                    objectToMove.position = pos;
                }
            }
        }

        private void DragEnd()
        {
            isDragging = false;
            objectToMove = null;
        }

        private void OnDrawGizmosSelected()
        {
            if(!_showGizmos) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
