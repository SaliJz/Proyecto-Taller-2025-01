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
        CurrentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);

    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("CurrentLevelIndex", CurrentLevelIndex);
        PlayerPrefs.Save();
    }

    public void StartTutorial()
    {
        CurrentLevelIndex = 0; // El tutorial es el índice 0 en la lista 'levelProgression'
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void StartFromLevel1()
    {
        CurrentLevelIndex = 1; // El Nivel 1 es el índice 1 en la lista 'levelProgression'
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void OnLevelCompleted()
    {
        CurrentLevelIndex++;
        SaveGameData();
        StartCoroutine(TransitionToShop());
    }

    private void LoadLevelByIndex(int index)
    {
        if (index >= 0 && index < levelProgression.Count)
        {
            string sceneToLoad = levelProgression[index];

            // Usamos la transición para cargar la escena.
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
            // Si el índice está fuera de rango (por ejemplo, después del jefe), vamos a la pantalla de victoria.
            Debug.Log("Progresión completada. Cargando escena de victoria.");
            SceneManager.LoadScene(victorySceneName);
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

    public void OnBossDefeated()
    {
        SceneManager.LoadScene(victorySceneName);
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
}