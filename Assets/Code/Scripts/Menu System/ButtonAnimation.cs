using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Title("Settings")]
    [SerializeField, MinValue(0f)] private float scaleUp = 1.1f;
    [SerializeField, MinValue(0f)] private float scaleDuration = 0.2f;
    private Transform target;
    private Vector3 initialScale;

    private void Start()
    {
        target = transform;
        initialScale = target.localScale;
    }

    /// <summary>
    /// Called when the pointer enters the button area.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        target.DOScale(scaleUp, scaleDuration);
    }

    /// <summary>
    /// Called when the pointer exits the button area.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        target.DOScale(initialScale, scaleDuration);
    }

    /// <summary>
    /// Called when the button is pressed.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        target.DOScale(scaleUp, scaleDuration);
    }

    /// <summary>
    /// Called when the button is released.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        target.DOScale(initialScale, scaleDuration);
    }
}
