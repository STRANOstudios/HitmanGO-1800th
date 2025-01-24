using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator[] objects;
    public float minDelay = 0f;
    public float maxDelay = 2f;

    void Start()
    {
        foreach (Animator anima in objects)
        {
            anima.enabled = false;
        }

        StartCoroutine(StartAnimationWithDelay(objects));
    }

    private IEnumerator StartAnimationWithDelay(Animator[] animator)
    {
        foreach (Animator anima in animator)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
            anima.enabled = true;
        }
    }
}
