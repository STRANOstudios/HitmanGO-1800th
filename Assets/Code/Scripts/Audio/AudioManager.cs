using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Title("Settings")]
        [SerializeField, InlineEditor, Required] private AudioData audioData; // ScriptableObject reference
        [SerializeField, Required] private GameObject audioSourcePrefab; // Prefab for AudioSource pooling

        [Title("Debug")]
        [Button]
        public void PlayMusic()
        {
            PlayMusic("music");
        }

        [Button]
        public void PlaySFX()
        {
            PlaySFX("shoot", transform);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Play a Music clip by its unique key.
        /// </summary>
        public void PlayMusic(string key)
        {
            if (key == null) return;

            var clip = audioData.GetMusicClip(key);
            if (clip == null)
            {
                Debug.LogWarning($"Music clip with key '{key}' not found.");
                return;
            }

            PlayAudio(clip, audioData.GetMusicMixerGroup());
        }

        /// <summary>
        /// Play an Sfx clip by its unique key.
        /// </summary>
        public void PlaySFX(string key, Transform parent = null)
        {
            if (key is null or "") return;

            var clip = audioData.GetSFXClip(key);
            if (clip == null)
            {
                Debug.LogWarning($"SFX clip with key '{key}' not found.");
                return;
            }

            PlayAudio(clip, audioData.GetSFXMixerGroup(), parent);
        }

        /// <summary>
        /// Plays a given AudioClip using a pooled AudioSource.
        /// </summary>
        private void PlayAudio(AudioClip clip, AudioMixerGroup group, Transform parent = null)
        {
            var pooledAudio = ObjectPooler.Instance.Get(audioSourcePrefab);
            var audioSource = pooledAudio.GetComponent<AudioSource>();

            if (parent != null) pooledAudio.transform.position = parent.position;

            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = group ?? null;
            audioSource.Play();

            // Return the object to the pool after the clip ends
            StartCoroutine(ReturnToPool(audioSource, clip.length));
        }

        /// <summary>
        /// Returns the AudioSource GameObject to the pool after playback.
        /// </summary>
        private IEnumerator ReturnToPool(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);

            source.clip = null; // Clear the clip
            ObjectPooler.Instance.ReturnToPool(source.gameObject);
        }
    }
}
