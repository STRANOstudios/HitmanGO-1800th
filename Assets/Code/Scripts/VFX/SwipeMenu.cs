using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private GameObject scrollbar;
    [SerializeField] private List<Image> images;
    [SerializeField, MinValue(0f)] private float interpolationSpeed = 10f;
    [SerializeField, ColorPalette] private Color selectedColor = Color.red;

    private float scroll_pos = 0f;
    private float[] pos;
    private float distance;
    private int oldIndex = 0;

    private void Start()
    {
        pos = new float[transform.childCount];
        distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
        images[0].color = Color.red;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    if (scroll_pos < pos[i] + (distance / 2f) && scroll_pos > pos[i] - (distance / 2f))
                    {
                        StartCoroutine(MoveScrollbar(pos[i]));
                        images[oldIndex].color = Color.white;
                        oldIndex = i;
                        images[i].color = selectedColor;
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator MoveScrollbar(float target)
    {
        float start = scrollbar.GetComponent<Scrollbar>().value;
        float timeElapsed = 0f;

        while (timeElapsed < 1f)
        {
            scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(start, target, timeElapsed);
            timeElapsed += Time.deltaTime * interpolationSpeed;
            yield return null;
        }
        scrollbar.GetComponent<Scrollbar>().value = target;
    }
}
