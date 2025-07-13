using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class SettingsService
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string VoiceVolumeKey = "VoiceVolume";
    private const string MuteKey = "Mute";
    private const string ResolutionKey = "Resolution";
    private const string VSyncKey = "VSync";
    private const string FullscreenKey = "Fullscreen";
    private const string SensitivityKey = "Sensitivity";
    private const string ShowFpsKey = "ShowFps";

    public static float MasterVolume = 1f;
    public static float SfxVolume = 1f;
    public static float Sensitivity = 5f;
    public static bool Mute = false;
    public static bool VSync = true;
    public static bool IsFullscreen = true;
    public static bool ShowFps = true;
    public static int ResolutionIndex = 0;

    // Resoluciones estándar como en SettingsController
    private static readonly List<Vector2Int> StandardResolutions = new()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
        new Vector2Int(7680, 4320)
    };

    public static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        Mute = PlayerPrefs.GetInt(MuteKey, 0) == 1;
        ResolutionIndex = PlayerPrefs.GetInt(ResolutionKey, 2); // por defecto 1920x1080
        VSync = PlayerPrefs.GetInt(VSyncKey, 1) == 1;
        IsFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        Sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        ShowFps = PlayerPrefs.GetInt(ShowFpsKey, 1) == 1;
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SfxVolume);
        PlayerPrefs.SetInt(MuteKey, Mute ? 1 : 0);
        PlayerPrefs.SetInt(ResolutionKey, ResolutionIndex);
        PlayerPrefs.SetInt(VSyncKey, VSync ? 1 : 0);
        PlayerPrefs.SetInt(FullscreenKey, IsFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat(SensitivityKey, Sensitivity);
        PlayerPrefs.SetInt(ShowFpsKey, ShowFps ? 1 : 0);
    }

    public static void Apply(AudioMixer audioMixer)
    {
        float masterDb = ConvertToDecibels(MasterVolume);
        float sfxDb = ConvertToDecibels(SfxVolume);

        audioMixer.SetFloat("VolMaster", Mute ? -80f : masterDb);
        audioMixer.SetFloat("VolSFX", Mute ? -80f : sfxDb);

        AudioListener.pause = Mute;

        Vector2Int selectedRes = StandardResolutions[Mathf.Clamp(ResolutionIndex, 0, StandardResolutions.Count - 1)];
        Screen.SetResolution(selectedRes.x, selectedRes.y, IsFullscreen);
        QualitySettings.vSyncCount = VSync ? 1 : 0;
    }

    public static bool HasChanges(
        float masterVolume,
        float sfxVolume,
        bool mute,
        int resolutionIndex,
        bool vsync,
        bool fullscreen,
        float sensitivity)
    {
        return !Mathf.Approximately(masterVolume, MasterVolume) ||
               !Mathf.Approximately(sfxVolume, SfxVolume) ||
               mute != Mute ||
               resolutionIndex != ResolutionIndex ||
               vsync != VSync ||
               fullscreen != IsFullscreen ||
               !Mathf.Approximately(sensitivity, Sensitivity);
    }

    private static float ConvertToDecibels(float linearValue)
    {
        return Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f;
    }

    public static SettingsData Clone()
    {
        return new SettingsData
        {
            MasterVolume = MasterVolume,
            SfxVolume = SfxVolume,
            Sensitivity = Sensitivity,
            ResolutionIndex = ResolutionIndex,
            IsFullscreen = IsFullscreen,
            VSync = VSync,
            ShowFps = ShowFps,
            Mute = Mute
        };
    }

    public static void ResetToDefault()
    {
        MasterVolume = 1f;
        SfxVolume = 1f;
        Sensitivity = 5f;
        Mute = false;
        VSync = true;
        IsFullscreen = true;
        ResolutionIndex = 2;
        ShowFps = true;
    }
}

public class SettingsData
{
    public float MasterVolume;
    public float SfxVolume;
    public float Sensitivity;
    public int ResolutionIndex;
    public bool IsFullscreen;
    public bool VSync;
    public bool ShowFps;
    public bool Mute;
}
