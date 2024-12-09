using UnityEngine;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
public class ScrollingText : MonoBehaviour
{
    [Title("Scrolling Settings")]
    [SerializeField, MinValue(0f)] private float scrollSpeed = 50f;
    [SerializeField, MinValue(0f)] private float touchHoldThreshold = 0.2f;
    [SerializeField, MinValue(0f)] private float swipeSpeed = 200f;

    [Title("Debug")]
    [SerializeField] private bool _debug = false;

    private RectTransform rectTransform;
    private Vector3 startPosition;
    private float timeTouchReleased = 0f;
    private bool isTouching = false;
    private bool isAnimating = true;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        HandleTouch();

        if (isAnimating)
        {
            AnimateScroll();
        }
        else
        {
            timeTouchReleased += Time.deltaTime;
            if (timeTouchReleased > touchHoldThreshold && !isTouching)
            {
                isAnimating = true;
            }
        }
    }

    private void HandleTouch()
    {
        if (Input.touchCount > 0)
        {
            if(_debug) Debug.Log("Touching");

            Touch touch = Input.GetTouch(0);
            Vector2 touchDelta = touch.deltaPosition;

            if (touch.phase == TouchPhase.Moved)
            {
                if (_debug) Debug.Log("Moving");

                isTouching = true;
                isAnimating = false;

                rectTransform.anchoredPosition += new Vector2(0, touchDelta.y * swipeSpeed * Time.deltaTime);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (_debug) Debug.Log("Released");

                isTouching = false;

                timeTouchReleased = 0f;
            }
        }
    }

    private void AnimateScroll()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        if (rectTransform.anchoredPosition.y > Screen.height)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }

    [Button(ButtonSizes.Medium), DisableInEditorMode]
    public void StartScrolling()
    {
        isAnimating = true;
    }

    [Button(ButtonSizes.Medium), DisableInEditorMode]
    public void StopScrolling()
    {
        isAnimating = false;
    }
}
