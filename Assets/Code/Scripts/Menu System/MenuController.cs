using UnityEngine;

[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}