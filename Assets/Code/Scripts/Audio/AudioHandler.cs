using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class AudioHandler : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private string m_clipName = "";
        [SerializeField] private bool m_isMusic = false;

        public void OnTrigger()
        {
            if (m_isMusic)
                PlayAudio();
            else
                PlaySfx();
        }

        public void PlayAudio() => AudioManager.Instance.ChangeMusic(m_clipName);

        public void PlaySfx() => AudioManager.Instance.PlaySFX(m_clipName);
    }
}
