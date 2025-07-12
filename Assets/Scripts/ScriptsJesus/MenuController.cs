using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button startLevel1Button;
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
    //[SerializeField] private string levelSceneName = "";
    [SerializeField] private string creditsSceneName = "Creditos";

    //[SerializeField] private Toggle replayTutorialsToggle;

    private void Awake()
    {
        tutorialButton.onClick.AddListener(PlayTutorial);
        startLevel1Button.onClick.AddListener(PlayFromLevel1);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }
        /*
        if (GameManager.Instance != null && replayTutorialsToggle != null)
        {
            replayTutorialsToggle.gameObject.SetActive(GameManager.Instance.TutorialsCompleted);
        }
        */
    }

    public void PlayTutorial()
    {
        PlayButtonAudio();
        GameManager.Instance?.StartTutorial();
    }

    public void PlayFromLevel1()
    {
        PlayButtonAudio();
        GameManager.Instance?.StartFromLevel1();
        //bool forceTutorials = replayTutorialsToggle != null && replayTutorialsToggle.isOn;
        //GameManager.Instance?.StartGame(forceTutorials);
    }

    public void OpenCredits()
    {
        PlayButtonAudio();
        SceneManager.LoadScene(creditsSceneName);
    }

    public void OpenSettings()
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