using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button returnButton;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumenSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Toggles")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Toggle fpsToggle;
    [SerializeField] private Toggle muteToggle;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI voiceVolumeText;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip toggleClip;
    [SerializeField] private AudioClip sliderClip;

    private readonly List<Vector2Int> standardResolutions = new()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
        new Vector2Int(7680, 4320)
    };

    private SettingsData tempSettings;

    private void Awake()
    {
        PopulateResolutionDropdown();
        RegisterListeners();
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private IEnumerator ShowConfirmation(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        confirmationText.gameObject.SetActive(false);
    }

    private void PopulateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(standardResolutions.Select(r => $"{r.x} x {r.y}").ToList());
    }

    private void RegisterListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(val => UpdateVolumeText(masterVolumeText, val));
        musicVolumeSlider.onValueChanged.AddListener(val => UpdateVolumeText(musicVolumeText, val));
        sfxVolumeSlider.onValueChanged.AddListener(val => UpdateVolumeText(sfxVolumeText, val));
        //voiceVolumenSlider.onValueChanged.AddListener(val => UpdateVolumeText(voiceVolumeText, val));
        sensitivitySlider.onValueChanged.AddListener(val => UpdateSensitivityText(val));

        muteToggle.onValueChanged.AddListener(value => tempSettings.Mute = value);
        vsyncToggle.onValueChanged.AddListener(value => tempSettings.VSync = value);
        fullscreenToggle.onValueChanged.AddListener(value => tempSettings.IsFullscreen = value);
        fpsToggle.onValueChanged.AddListener(value =>
        {
            tempSettings.ShowFps = value;
        });

        applyButton.onClick.AddListener(ApplySettings);
        resetButton.onClick.AddListener(ResetSettings);
        returnButton.onClick.AddListener(HandleReturn);
    }

    private bool HasUnsavedChanges()
    {
        return SettingsService.HasChanges(
            masterVolumeSlider.value,
            musicVolumeSlider.value,
            sfxVolumeSlider.value,
            muteToggle.isOn,
            resolutionDropdown.value,
            vsyncToggle.isOn,
            fullscreenToggle.isOn,
            sensitivitySlider.value
        );
    }

    private void LoadSettings()
    {
        SettingsService.Load();
        tempSettings = SettingsService.Clone();

        masterVolumeSlider.value = tempSettings.MasterVolume;
        musicVolumeSlider.value = tempSettings.MusicVolume;
        sfxVolumeSlider.value = tempSettings.SfxVolume;
        sensitivitySlider.value = tempSettings.Sensitivity;

        fullscreenToggle.isOn = tempSettings.IsFullscreen;
        vsyncToggle.isOn = tempSettings.VSync;
        fpsToggle.isOn = tempSettings.ShowFps;
        muteToggle.isOn = tempSettings.Mute;

        resolutionDropdown.value = Mathf.Clamp(tempSettings.ResolutionIndex, 0, standardResolutions.Count - 1);
        resolutionDropdown.RefreshShownValue();

        fpsText.gameObject.SetActive(tempSettings.ShowFps);

        UpdateVolumeText(masterVolumeText, tempSettings.MasterVolume);
        UpdateVolumeText(musicVolumeText, tempSettings.MusicVolume);
        UpdateVolumeText(sfxVolumeText, tempSettings.SfxVolume);
        //UpdateVolumeText(voiceVolumeText, tempSettings.VoiceVolume);
        UpdateSensitivityText(tempSettings.Sensitivity);
    }

    private void ApplySettings()
    {
        PlayButtonAudio();

        SettingsService.MasterVolume = masterVolumeSlider.value;
        SettingsService.MusicVolume = musicVolumeSlider.value;
        SettingsService.SfxVolume = sfxVolumeSlider.value;
        SettingsService.Sensitivity = sensitivitySlider.value;
        SettingsService.IsFullscreen = fullscreenToggle.isOn;
        SettingsService.VSync = vsyncToggle.isOn;
        SettingsService.ShowFps = fpsToggle.isOn;
        SettingsService.Mute = muteToggle.isOn;
        SettingsService.ResolutionIndex = resolutionDropdown.value;

        SettingsService.Save();
        SettingsService.Apply(audioMixer);

        fpsText.gameObject.SetActive(SettingsService.ShowFps);

        StartCoroutine(ShowConfirmation("Configuración aplicada"));
    }

    private void ResetSettings()
    {
        PlayButtonAudio();

        SettingsService.ResetToDefault();
        SettingsService.Save();
        SettingsService.Apply(audioMixer);
        LoadSettings();

        StartCoroutine(ShowConfirmation("Configuración restaurada"));
    }

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        text.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    private void UpdateSensitivityText(float value)
    {
        sensitivityValueText.text = value.ToString("0.0");
    }

    public void PlayButtonAudio() => PlayClip(buttonClip);
    public void PlayToggleAudio() => PlayClip(toggleClip);
    public void PlaySliderAudio() => PlayClip(sliderClip);

    private void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    private void HandleReturn()
    {
        PlayButtonAudio();

        if (HasUnsavedChanges())
        {
            StartCoroutine(ShowConfirmation("Cambios no guardados"));
            return;
        }
        else
        {
            confirmationText.gameObject.SetActive(false);
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }
}