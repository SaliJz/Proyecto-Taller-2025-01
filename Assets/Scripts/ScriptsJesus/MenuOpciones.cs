using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuOpciones : MonoBehaviour
{
    public string nombreEscenaRetry = "MenuPrincipal";

    [Header("SCREEN")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private int currentResolutionIndex;

    [Header("SOUND")]
    public Slider volumeSlider;
    public Slider sfxSlider;

    [Header("GAMEPLAY")]
    public Slider sensitivitySlider;

     void Start()
    {
        // Cargar resoluciones
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        // Opcional: Cargar valores previos
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 1f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);

    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void ApplyResolution()
    {
        SetResolution(resolutionDropdown.value);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetSFXVolume(float sfx)
    {
        // Suponiendo que tienes control independiente de SFX
        PlayerPrefs.SetFloat("SFX", sfx);
    }

    public void SetSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(nombreEscenaRetry);
    }




}
