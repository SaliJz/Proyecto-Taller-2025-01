using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Toggles")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Toggle fpsToggle;
    [SerializeField] private Toggle muteToggle;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource masterSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip audioClip;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button returnButton;

    [SerializeField] private TextMeshProUGUI fpsText;

    private float deltaTime;

    private readonly List<Vector2Int> standardResolutions = new List<Vector2Int>
    { 
    new Vector2Int(1280, 720),    // SD
    new Vector2Int(1366, 768),    // HD
    new Vector2Int(1920, 1080),   // Full HD
    new Vector2Int(2560, 1440),   // QHD
    new Vector2Int(3840, 2160),   // 4K
    new Vector2Int(7680, 4320)    // 8K
    };

    private void Awake()
    {
        var options = new List<string>();
        foreach (var r in standardResolutions)
        {
            options.Add($"{r.x} x {r.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        masterVolumeSlider.onValueChanged.AddListener(v => SettingsService.MasterVolume = v);
        sfxVolumeSlider.onValueChanged.AddListener(v => SettingsService.SfxVolume = v);
        sensitivitySlider.onValueChanged.AddListener(v => SettingsService.Sensitivity = v);

        fullscreenToggle.onValueChanged.AddListener(isOn => Screen.fullScreen = isOn);
        vsyncToggle.onValueChanged.AddListener(_ => VsyncToggle());
        fpsToggle.onValueChanged.AddListener(isOn => fpsText.gameObject.SetActive(isOn));
        muteToggle.onValueChanged.AddListener(_ => ToggleMute());

        resolutionDropdown.onValueChanged.AddListener(i => SettingsService.ResolutionIndex = i);

        applyButton.onClick.AddListener(ApplySettings);
        resetButton.onClick.AddListener(LoadSettings);
        returnButton.onClick.AddListener(ReturnButton);
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void Update()
    {
        if (!fpsToggle.isOn) return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fpsText.text = $"FPS: {Mathf.CeilToInt(1.0f / deltaTime)}";
    }

    private void LoadSettings()
    {
        SettingsService.Load();

        masterVolumeSlider.value = SettingsService.MasterVolume;
        sfxVolumeSlider.value = SettingsService.SfxVolume;
        sensitivitySlider.value = SettingsService.Sensitivity;

        fullscreenToggle.isOn = Screen.fullScreen;
        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        fpsToggle.isOn = fpsText.gameObject.activeSelf;
        muteToggle.isOn = SettingsService.MasterVolume <= 0.001f;

        resolutionDropdown.value = Mathf.Clamp(SettingsService.ResolutionIndex, 0, standardResolutions.Count - 1);
        resolutionDropdown.RefreshShownValue();
    }

    private void ApplySettings()
    {
        SettingsService.Save();
        SettingsService.Apply(audioMixer, vsyncToggle);
    }

    private void ReturnButton()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void VsyncToggle()
    {
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
    }

    private void ToggleMute()
    {
        if (muteToggle.isOn)
        {
            audioMixer.SetFloat("VolMaster", -80);
            audioMixer.SetFloat("VolSFX", -80);
        }
        else
        {
            audioMixer.SetFloat("VolMaster", Mathf.Log10(masterVolumeSlider.value) * 20);
            audioMixer.SetFloat("VolSFX", Mathf.Log10(sfxVolumeSlider.value) * 20);
        }
    }
}
