using System.Linq;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    [SerializeField] string[] sceneComponents;
    [SerializeField] string[] sceneNames;

    public void LoadScene(int index)
    {
        index--;
        SceneLoader.Instance.LoadMultipleScenes(sceneComponents.Concat(new[] { sceneNames[index] }).ToArray());
    }

    public void LoadScene()
    {
        SceneLoader.Instance.LoadScene(sceneComponents[0]);
    }
}
