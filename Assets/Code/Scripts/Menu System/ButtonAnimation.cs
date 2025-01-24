using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Title("Settings")]
    [SerializeField, MinValue(0f)] private float scaleUp = 1.1f;
    [SerializeField, MinValue(0f)] private float effectDuration = 0.2f;
    [SerializeField] private Image button;
    private Transform target;
    private Vector3 initialScale;

    private void Start()
    {
        button.DOFade(0f, 0f);
        target = transform;
        initialScale = target.localScale;
    }

    /// <summary>
    /// Called when the pointer enters the button area.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        target.DOScale(scaleUp, effectDuration);
    }

    /// <summary>
    /// Called when the pointer exits the button area.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        target.DOScale(initialScale, effectDuration);
    }

    /// <summary>
    /// Called when the button is pressed.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        target.DOScale(initialScale, effectDuration);
        button.DOFade(1f, effectDuration);
    }

    /// <summary>
    /// Called when the button is released.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        target.DOScale(initialScale, effectDuration);
        button.DOFade(0f, effectDuration).SetDelay(effectDuration);
    }
}
