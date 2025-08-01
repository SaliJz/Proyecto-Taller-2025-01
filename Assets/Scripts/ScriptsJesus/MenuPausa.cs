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
    [SerializeField] private string mainSceneName;

    [Header("Audio")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioClip buttonClip;

    private ShopController shopController;
    private ClipManager clipManager;
    private bool isDead = false;
    private bool pausedGame = false;

    private void Awake()
    {
        if (restartButton == null || mainMenuButton == null || settingsButton == null || quitButton == null)
        {
            return;
        }
        restartButton.onClick.AddListener(ResumeGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        settingsButton.onClick.AddListener(ShowSettings);
        quitButton.onClick.AddListener(QuitGame);

        shopController = FindObjectOfType<ShopController>();
        clipManager = FindObjectOfType<ClipManager>();
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (shopController != null && shopController.ShopPauseGame) return;
        if (clipManager != null && clipManager.ClipPauseGame) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !isDead)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !pausedGame)
            {
                PauseGame();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && pausedGame)
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        PlayButtonAudio();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        pausedGame = true;
    }

    public void ResumeGame()
    {
        PlayButtonAudio();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        pausedGame = false;
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
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitGame()
    {
        PlayButtonAudio();
        Application.Quit();
    }

    public void SetIsDead(bool isDead)
    {
        this.isDead = isDead;
    }

    private void PlayButtonAudio()
    {
        if (SFXSource != null && buttonClip != null)
        {
            SFXSource.PlayOneShot(buttonClip);
        }
    }
}
