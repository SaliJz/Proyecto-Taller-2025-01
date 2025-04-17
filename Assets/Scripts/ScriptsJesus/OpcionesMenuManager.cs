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
    }


    void SetVolume(float value)
    {
        // Asume que tienes un AudioMixer
        Debug.Log("Volumen general: " + value);
        // Aquí podrías usar un AudioMixer para cambiar el volumen real
    }

    void SetSFX(float value)
    {
        Debug.Log("Volumen de efectos: " + value);
    }

    void SetSensitivity(float value)
    {
        Debug.Log("Sensibilidad: " + value);
        
    }

    void ReturnToMainMenu()
    {
        
        SceneManager.LoadScene("MenuPrincipalJesus");
    }




}
