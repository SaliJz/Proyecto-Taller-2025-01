using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public static class SettingsService
{
    private static readonly string MasterVolumeKey = "VolumenGeneral";
    private static readonly string SfxVolumeKey = "VolumenSFX";
    private static readonly string SensitivityKey = "Sensibilidad";
    private static readonly string ResolutionIndexKey = "ResolucionIndex";

    public static float MasterVolume { get; set; } = 1f;
    public static float SfxVolume { get; set; } = 1f;
    public static float Sensitivity { get; set; } = 1f;
    public static int ResolutionIndex { get; set; } = 0;

    public static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        Sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        ResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, 0);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.SetFloat(SensitivityKey, Sensitivity);
        PlayerPrefs.SetInt(ResolutionIndexKey, ResolutionIndex);
        PlayerPrefs.Save();
    }

    public static void Apply(AudioMixer mixer, Toggle vsyncToggle)
    {
        // Audio
        mixer.SetFloat("VolMaster", Mathf.Log10(MasterVolume) * 20);
        mixer.SetFloat("VolSFX", Mathf.Log10(SfxVolume) * 20);

        // Sensibilidad
        // ej: InputManager.Instance.SetSensitivity(Sensitivity);

        // Resolución
        Resolution[] res = Screen.resolutions;
        int idx = Mathf.Clamp(ResolutionIndex, 0, res.Length - 1);
        Screen.SetResolution(res[idx].width, res[idx].height, Screen.fullScreen);

        // VSync
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
    }
}
