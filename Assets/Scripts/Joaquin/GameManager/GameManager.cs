using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de Niveles")]
    [SerializeField] private int bossLevelIndex = 9;
    [SerializeField] private string bossSceneName = "JeanPierre_Jefe";
    //[SerializeField] private string mainMenuSceneName = "MenuPrincipal";
    [SerializeField] private string victorySceneName = "Creditos";
    [SerializeField] private string defeatSceneName = "GameOver";
    [SerializeField] private string[] reusableLevelSceneNames = { "Nivel1 Variante", "Nivel2 Variante" };

    private bool tutorialsCompleted = false;
    public int CurrentLevelIndex { get; private set; }
    public bool GameCompletedOnce { get; private set; }
    public bool TutorialsCompleted { get; private set; }

    private void Awake()
    {
        Debug.Log($"[GameManager] Tutorials Completed: {tutorialsCompleted}");

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
        /*
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
        {
            tutorialsCompleted = !tutorialsCompleted;
            Debug.Log($"Tutorials Completed: {tutorialsCompleted}");
        }
        */
    }


    private void LoadGameData()
    {
        CurrentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        GameCompletedOnce = PlayerPrefs.GetInt("GameCompletedOnce", 0) == 1;
        tutorialsCompleted = PlayerPrefs.GetInt("TutorialsCompleted", 0) == 1;

    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("CurrentLevelIndex", CurrentLevelIndex);
        PlayerPrefs.SetInt("GameCompletedOnce", GameCompletedOnce ? 1 : 0);
        PlayerPrefs.SetInt("TutorialsCompleted", tutorialsCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void StartGame(bool forceTutorials)
    {
        if (forceTutorials)
        {
            CurrentLevelIndex = 0;
        }
        else if (tutorialsCompleted)
        {
            CurrentLevelIndex = 2;
        }
        else
        {
            CurrentLevelIndex = 0;
        }

        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void OnBossDefeated()
    {
        GameCompletedOnce = true;
        CurrentLevelIndex = 2;
        SaveGameData();

        SceneManager.LoadScene(victorySceneName);
    }

    public void OnLevelCompleted()
    {
        if (CurrentLevelIndex == 1) tutorialsCompleted = true;
        CurrentLevelIndex++;
        SaveGameData();

        StartCoroutine(TransitionToShop());
    }

    private IEnumerator TransitionToShop()
    {
        SceneTransition transition = FindObjectOfType<SceneTransition>();
        if (transition != null)
        {
            yield return transition.Fade(1f);
        }

        FindObjectOfType<ShopController>()?.OpenShop();

        if (transition != null)
        {
            yield return transition.Fade(0f);
        }
    }

    public void LoadNextLevelAfterShop()
    {
        LoadLevelByIndex(CurrentLevelIndex);
    }

    private void LoadLevelByIndex(int index)
    {
        if (index >= bossLevelIndex)
        {
            SceneManager.LoadScene(bossSceneName);
            return;
        }

        string sceneToLoad;
        if (index == 0)
        {
            sceneToLoad = "Nivel1";
        }
        else if (index == 1)
        {
            sceneToLoad = "Nivel2";
        }
        else
        {
            int reusableIndex = (index - 2) % reusableLevelSceneNames.Length;
            sceneToLoad = reusableLevelSceneNames[reusableIndex];
        }

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

    public void SetForceTutorials(bool force)
    {
        if (force)
        {
            CurrentLevelIndex = 0;
            tutorialsCompleted = false;
        }
        else
        {
            LoadLevelByIndex(CurrentLevelIndex);
        }
    }

    public void PlayerDied()
    {
        SceneManager.LoadScene(defeatSceneName);
    }

    public void ReloadLastLevel()
    {
        LoadLevelByIndex(CurrentLevelIndex);
    }
}