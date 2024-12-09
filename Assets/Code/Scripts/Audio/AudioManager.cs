using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Title("Settings")]
        [SerializeField, InlineEditor, Required] private AudioData audioData;

        [Title("Debug")]
        [SerializeField] private bool _debug;

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

            if (_debug) Debug.Log($"Playing SFX: {key}");

            var clip = audioData.GetSFXClip(key);
            if (clip == null)
            {
                Debug.LogWarning($"SFX clip with key '{key}' not found.");
                return;
            }

            PlayAudio(clip, audioData.GetSFXMixerGroup(), parent);
        }

        /// <summary>
        /// Play an Sfx clip by its unique key.
        /// </summary>
        public void PlaySfx(string key)
        {
            if (_debug) Debug.Log($"Playing SFX: {key}");
            PlaySFX(key);
        }

        /// <summary>
        /// Plays a given AudioClip using a pooled AudioSource.
        /// </summary>
        private void PlayAudio(AudioClip clip, AudioMixerGroup group, Transform parent = null)
        {
            if (_debug) Debug.Log($"Playing Audio: {clip.name}");

            var pooledAudio = ObjectPooler.Instance.Get("AudioSource", clip.length);
            var audioSource = pooledAudio.GetComponent<AudioSource>();

            if (parent != null) pooledAudio.transform.position = parent.position;

            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = group ?? null;
            audioSource.Play();
        }
    }
}
