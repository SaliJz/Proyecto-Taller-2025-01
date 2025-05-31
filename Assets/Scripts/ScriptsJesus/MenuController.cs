using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Audio")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioClip buttonClip;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Scenes")]
    [SerializeField] private string levelSceneName = "Jesus";
    [SerializeField] private string creditsSceneName = "Creditos";

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playButton.onClick.AddListener(StartGame);
        creditsButton.onClick.AddListener(ShowCredits);
        quitButton.onClick.AddListener(QuitGame);
        
        if (settingsPanel != null)
        {
            settingsButton.onClick.AddListener(ShowOpciones);
        }
    }

    public void StartGame()
    {
        PlayButtonAudio();

        SceneManager.LoadScene(levelSceneName);
    }

    public void ShowCredits()
    {
        PlayButtonAudio();

        SceneManager.LoadScene(creditsSceneName);  
    }

    public void ShowOpciones()
    {
        PlayButtonAudio();

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PlayButtonAudio();
        Application.Quit();
    }

    private void PlayButtonAudio()
    {
        if (buttonClip != null)
        {
            SFXSource.PlayOneShot(buttonClip);
        }
    }
}
