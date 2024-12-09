using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioData", menuName = "Audio/AudioData")]
public class AudioData : SerializedScriptableObject
{
    [Title("Music Clips")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [OdinSerialize, Tooltip("Dictionary of music clips, accessible by unique string keys.")]
    [DictionaryDrawerSettings(KeyLabel = "Name", ValueLabel = "Audio Clip")]
    private Dictionary<string, AudioClip> musicClips = new();

    [Title("SFX Clips")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [OdinSerialize, Tooltip("Dictionary of sound effects clips, accessible by unique string keys.")]
    [DictionaryDrawerSettings(KeyLabel = "Name", ValueLabel = "Audio Clip")]
    private Dictionary<string, AudioClip> sfxClips = new();

    /// <summary>
    /// Get a music clip by its unique key.
    /// </summary>
    public AudioClip GetMusicClip(string key)
    {
        return musicClips.TryGetValue(key, out var clip) ? clip : null;
    }

    /// <summary>
    /// Get an Sfx clip by its unique key.
    /// </summary>
    public AudioClip GetSFXClip(string key)
    {
        return sfxClips.TryGetValue(key, out var clip) ? clip : null;
    }

    /// <summary>
    /// Get _mixer group for music.
    /// </summary>
    public AudioMixerGroup GetMusicMixerGroup()
    {
        return musicMixerGroup;
    }

    /// <summary>
    /// Get _mixer group for Sfx.
    /// </summary>
    public AudioMixerGroup GetSFXMixerGroup()
    {
        return sfxMixerGroup;
    }

    /// <summary>
    /// Add a music clip with a specific key.
    /// </summary>
    public void AddMusicClip(string key, AudioClip clip)
    {
        if (!musicClips.ContainsKey(key))
            musicClips.Add(key, clip);
    }

    /// <summary>
    /// Add an Sfx clip with a specific key.
    /// </summary>
    public void AddSFXClip(string key, AudioClip clip)
    {
        if (!sfxClips.ContainsKey(key))
            sfxClips.Add(key, clip);
    }
}
