using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de Niveles")]
    [SerializeField] private List<string> levelProgression;

    [Header("Configuración de Escenas")]
    [SerializeField] private string mainMenuSceneName = "MenuPrincipal";
    [SerializeField] private string victorySceneName = "Creditos";
    [SerializeField] private string defeatSceneName = "GameOver";

    public int CurrentLevelIndex { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGameData();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
        {
            OnLevelCompleted();
        }
    }

    private void LoadGameData()
    {
        int savedIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        if (savedIndex < 0 || savedIndex >= levelProgression.Count)
        {
            savedIndex = 0;
        }
        else
        {
            CurrentLevelIndex = savedIndex;
        }
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("CurrentLevelIndex", CurrentLevelIndex);
        PlayerPrefs.Save();
    }

    public void StartTutorial()
    {
        CurrentLevelIndex = 0;
        SaveGameData();
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void StartFromLevel1()
    {
        CurrentLevelIndex = 1;
        SaveGameData();
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void OnLevelCompleted()
    {
        CurrentLevelIndex++;
        SaveGameData();

        ShopManager shopController = FindObjectOfType<ShopManager>();

        if (shopController != null)
        {
            shopController.OpenShop();
        }
        else
        {
            LoadNextLevelAfterShop();
        }
    }

    public void LoadNextLevelAfterShop()
    {
        LoadLevelByIndex(CurrentLevelIndex);
    }

    private void LoadLevelByIndex(int index)
    {
        if (index >= 0 && index < levelProgression.Count)
        {
            string sceneToLoad = levelProgression[index];
            SceneTransition transition = FindObjectOfType<SceneTransition>();
            if (transition != null)
            {
                transition.LoadSceneWithFade(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            SceneManager.LoadScene(victorySceneName);
        }
    }

    public void PlayerDied()
    {
        SaveGameData();
        SceneManager.LoadScene(defeatSceneName);
    }

    public void ReloadLastLevel()
    {
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void OnBossDefeated()
    {
        SceneManager.LoadScene(victorySceneName);
    }
}