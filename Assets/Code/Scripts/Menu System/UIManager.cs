using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void BtnPulse(Transform obj)
    {
        obj.DOScale(1.1f, 0.2f).OnComplete(() => obj.DOScale(1f, 0.2f));
    }
}
