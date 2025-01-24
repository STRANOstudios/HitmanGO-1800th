using Audio;
using UnityEngine;

public class AudioHandlerGameStatus : MonoBehaviour
{
    public string m_clipNameInLevel;
    public string m_clipNameEndLevel = "";

    private void Awake()
    {

        if (m_clipNameEndLevel == "")
        {
            m_clipNameEndLevel = AudioManager.Instance.GetCurrentMusic;
        }

        AudioManager.Instance.ChangeMusic(m_clipNameInLevel);
    }

    private void OnEnable()
    {
        GameManager.OnEndGame += Execute;
    }

    private void OnDisable()
    {
        GameManager.OnEndGame -= Execute;
    }

    private void Execute()
    {
        AudioManager.Instance.ChangeMusic(m_clipNameEndLevel);
    }
}
