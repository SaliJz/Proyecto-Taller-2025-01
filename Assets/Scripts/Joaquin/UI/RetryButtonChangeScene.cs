using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RetryButtonChangeScene : MonoBehaviour
{
    public static string SCENE_NAME = string.Empty;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private AudioSource fxSource;
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        PlaySoundButton();
        if (SCENE_NAME == string.Empty)
        {
            sceneTransition.LoadSceneWithFade("MenuPrincipal");
        }
        else
        {
            GameManager.Instance?.ReloadLastLevel();
        }
    }
    
    private void PlaySoundButton()
    {
        if (fxSource != null && clickSound != null)
        {
            fxSource.PlayOneShot(clickSound);
        }
    }
}