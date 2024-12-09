using UnityEngine;
using TMPro;
using System.Collections;
using Sirenix.OdinInspector;

public class DotsAnimation : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private TextMeshProUGUI dotsText;
    [SerializeField] private float delay = 0.5f;
    private string currentText;

    private void Start()
    {
        currentText = "";
        StartCoroutine(AnimateDots());
    }

    private IEnumerator AnimateDots()
    {
        while (true)
        {
            dotsText.text = ".";
            yield return new WaitForSeconds(delay);

            dotsText.text += " .";
            yield return new WaitForSeconds(delay);

            dotsText.text += " .";
            yield return new WaitForSeconds(delay);
        }
    }
}
