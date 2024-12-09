using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField, Required] private Image blackScreen;
    [SerializeField, MinValue(0f)] private float fadeDuration = 1f;
    [SerializeField, ColorPalette] private Color colorIn = Color.black;
    [SerializeField, ColorPalette] private Color colorOut = Color.black;

    private GameObject source;
    private GameObject target;

    public void TransitionIn(GameObject obj)
    {
        source = obj;
    }
    public void TransitionOut(GameObject obj)
    {
        target = obj;

        if(source != null && target != null)
        {
            StartCoroutine(Transition());
        }
    }

    private IEnumerator Transition()
    {
        yield return StartCoroutine(FadeIn());
        source?.SetActive(false);
        target?.SetActive(true);
        yield return StartCoroutine(FadeOut());

        source = null;
        target = null;
    }

    private IEnumerator FadeIn()
    {
        blackScreen.color = new(colorIn.r, colorIn.g, colorIn.b, 0);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        blackScreen.color = new Color(colorIn.r, colorIn.g, colorIn.b, 1);
    }

    private IEnumerator FadeOut()
    {
        blackScreen.color = new(colorOut.r, colorOut.g, colorOut.b, 1);
        float timerOut = 0f;
        while (timerOut < fadeDuration)
        {
            timerOut += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0, timerOut / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        blackScreen.color = new Color(colorOut.r, colorOut.g, colorOut.b, 0);
    }
}
