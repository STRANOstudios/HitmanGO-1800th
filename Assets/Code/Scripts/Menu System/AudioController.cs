using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;

[DisallowMultipleComponent]
public class AudioController : MonoBehaviour
{
    [Title("References")]
    [SerializeField, Required] private AudioMixer _mixer;

    [FoldoutGroup("Master")]
    [SerializeField] private Slider _volumeMaster;
    [FoldoutGroup("Master")]
    [SerializeField] private LocalizedString _masterMuteButton;

    [FoldoutGroup("Music")]
    [SerializeField] private Slider _volumeMusic;
    [FoldoutGroup("Music")]
    [SerializeField] private LocalizedString _muteMusicButton;

    [FoldoutGroup("Sfx")]
    [SerializeField] private Slider _volumeSFX;
    [FoldoutGroup("Sfx")]
    [SerializeField] private LocalizedString _muteSFXButton;

    [Title("Debug")]
    [SerializeField] private bool _debug;

    private bool isMasterMuted = false;
    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

    public float value = 0;

    private const string MASTER_KEY = "VolumeMaster";
    private const string MUSIC_KEY = "VolumeMusic";
    private const string SFX_KEY = "VolumeSFX";
    private const string MUTE_MASTER_KEY = "MuteMaster";
    private const string MUTE_MUSIC_KEY = "MuteMusic";
    private const string MUTE_SFX_KEY = "MuteSFX";

    private void OnEnable()
    {
        _volumeMaster?.onValueChanged.AddListener(OnMasterVolumeChanged);
        _volumeMusic?.onValueChanged.AddListener(OnMusicVolumeChanged);
        _volumeSFX?.onValueChanged.AddListener(OnSFXVolumeChanged);

        LoadSettings();
    }

    private void OnDisable()
    {
        _volumeMaster?.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        _volumeMusic?.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        _volumeSFX?.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    private void LoadSettings()
    {
        float masterVolume = SaveSystem.Exists(MASTER_KEY) ? SaveSystem.Load<float>(MASTER_KEY) : 0.75f;
        float musicVolume = SaveSystem.Exists(MUSIC_KEY) ? SaveSystem.Load<float>(MUSIC_KEY) : 0.75f;
        float sfxVolume = SaveSystem.Exists(SFX_KEY) ? SaveSystem.Load<float>(SFX_KEY) : 0.75f;

        isMasterMuted = SaveSystem.Exists(MUTE_MASTER_KEY) && SaveSystem.Load<bool>(MUTE_MASTER_KEY);
        isMusicMuted = SaveSystem.Exists(MUTE_MUSIC_KEY) && SaveSystem.Load<bool>(MUTE_MUSIC_KEY);
        isSFXMuted = SaveSystem.Exists(MUTE_SFX_KEY) && SaveSystem.Load<bool>(MUTE_SFX_KEY);

        if (_volumeMaster != null) _volumeMaster.value = masterVolume;
        if (_volumeMusic != null) _volumeMusic.value = musicVolume;
        if (_volumeSFX != null) _volumeSFX.value = sfxVolume;

        ApplyMute(MixerGroup.Master, isMasterMuted);
        ApplyMute(MixerGroup.Music, isMusicMuted);
        ApplyMute(MixerGroup.Sfx, isSFXMuted);

        UpdateMuteButtonText(); // Update the button text on load
    }

    private void UpdateMuteButtonText()
    {
        if (_debug) Debug.Log("Updating Mute Button Text");

        Debug.Log("To be implemented update text on mute button");
    }

    private void OnMasterVolumeChanged(float value)
    {
        ApplyVolume(MixerGroup.Master, value);
        SaveSystem.Save(value, MASTER_KEY);
    }

    private void OnMusicVolumeChanged(float value)
    {
        ApplyVolume(MixerGroup.Music, value);
        SaveSystem.Save(value, MUSIC_KEY);
    }

    private void OnSFXVolumeChanged(float value)
    {
        ApplyVolume(MixerGroup.Sfx, value);
        SaveSystem.Save(value, SFX_KEY);
    }

    public void ToggleMuteMaster()
    {
        isMasterMuted = !isMasterMuted;
        SaveSystem.Save(isMasterMuted, MUTE_MASTER_KEY);
        ApplyMute(MixerGroup.Master, isMasterMuted);
        UpdateMuteButtonText(); // Update the button text when toggled
    }

    public void ToggleMuteMusic()
    {
        isMusicMuted = !isMusicMuted;
        SaveSystem.Save(isMusicMuted, MUTE_MUSIC_KEY);
        ApplyMute(MixerGroup.Music, isMusicMuted);
        UpdateMuteButtonText(); // Update the button text when toggled
    }

    public void ToggleMuteSfx()
    {
        isSFXMuted = !isSFXMuted;
        SaveSystem.Save(isSFXMuted, MUTE_SFX_KEY);
        ApplyMute(MixerGroup.Sfx, isSFXMuted);
        UpdateMuteButtonText(); // Update the button text when toggled
    }

    private void ApplyMute(MixerGroup group, bool isMuted)
    {
        _mixer.SetFloat(group.ToString(), isMuted ? -80f : Mathf.Lerp(-80f, 20f, GetVolume(group)));
    }

    private void ApplyVolume(MixerGroup group, float value)
    {
        _mixer.SetFloat(group.ToString(), Mathf.Lerp(-80f, 20f, value));
    }

    private float GetVolume(MixerGroup group)
    {
        return group switch
        {
            MixerGroup.Master => _volumeMaster != null ? _volumeMaster.value : 0.75f,
            MixerGroup.Music => _volumeMusic != null ? _volumeMusic.value : 0.75f,
            MixerGroup.Sfx => _volumeSFX != null ? _volumeSFX.value : 0.75f,
            _ => 0.75f,
        };
    }

    [Serializable]
    public enum MixerGroup
    {
        Master,
        Music,
        Sfx
    }
}
