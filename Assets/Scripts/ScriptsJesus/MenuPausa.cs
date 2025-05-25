using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Texts")]
    [SerializeField] private string nombreEscenaMenuPrincipal;

    [Header("Audio")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioClip buttonClip;

    private bool isDead = false; 
    private bool juegoPausado = false;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (restartButton == null || mainMenuButton == null || settingsButton == null || quitButton == null)
        {
            Log("Uno o más botones no están asignados en el inspector.");
            return;
        }
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        settingsButton.onClick.AddListener(ShowSettings);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isDead)
        {
            if (!juegoPausado) PauseGame();
            else RestartGame();
        }
    }

    public void PauseGame()
    {
        PlayButtonAudio();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // Ocultar el cursor

        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        juegoPausado = true;
    }

    public void RestartGame()
    {
        PlayButtonAudio();
        Cursor.lockState = CursorLockMode.Locked; // Desbloquear el cursor
        Cursor.visible = false; // Mostrar el cursor

        Time.timeScale = 1f; 
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        juegoPausado = false;
    }

    public void ShowSettings()
    {
        PlayButtonAudio();
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        PlayButtonAudio();
        Time.timeScale = 1f; 
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    public void QuitGame()
    {
        Log("Saliendo del juego...");

        PlayButtonAudio();
        Application.Quit();
    }

    public void SetIsDead(bool isDead)
    {
        this.isDead = isDead; // Actualizar el estado de muerte
    }

    private void PlayButtonAudio()
    {
        if (SFXSource != null && buttonClip != null)
        {
            SFXSource.PlayOneShot(buttonClip);
        }
    }

#if UNITY_EDITOR
    private void Log(string message) => Debug.Log(message);
#endif
}
