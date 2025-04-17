using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OpcionesMenuManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Button applyButton;
    public Slider volumeSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;
    public Button retryButton;
    public TextMeshProUGUI volumeValueText;
    public TextMeshProUGUI sfxValueText;
    public TextMeshProUGUI sensitivityValueText;

    Resolution[] resolutions;

    void Start()
    {
        // Obtener resoluciones disponibles
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        // Llenar dropdown con resoluciones
        foreach (Resolution res in resolutions)
        {
            string option = res.width + " x " + res.height;
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }

        resolutionDropdown.RefreshShownValue();

        // Asignar eventos
        applyButton.onClick.AddListener(ApplyResolution);
        retryButton.onClick.AddListener(ReturnToMainMenu);
        volumeSlider.onValueChanged.AddListener(SetVolume);
        sfxSlider.onValueChanged.AddListener(SetSFX);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }


    void ApplyResolution()
    {
        Resolution selected = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selected.width, selected.height, FullScreenMode.Windowed);
        Debug.Log("Resolución cambiada a: " + selected.width + " x " + selected.height);
    }


    void SetVolume(float value)
    {
        volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        Debug.Log("Volumen general: " + value);
    }

    void SetSFX(float value)
    {
        sfxValueText.text = Mathf.RoundToInt(value * 100) + "%";
        Debug.Log("Volumen de efectos: " + value);
    }

    void SetSensitivity(float value)
    {
        sensitivityValueText.text = Mathf.RoundToInt(value * 100) + "%";
        Debug.Log("Sensibilidad: " + value);
        
    }

    void ReturnToMainMenu()
    {
        
        SceneManager.LoadScene("MenuPrincipalJesus");
    }




}
