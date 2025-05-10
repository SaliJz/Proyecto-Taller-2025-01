using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    [Header("Misi�n base")]
    [SerializeField] private List<Mission> baseMissions; // ScriptableObjects
    [SerializeField] private GameObject abilitySelectorUI;
    [SerializeField] private string nextSceneName = "VictoryScene";

    private int currentMissionIndex = 0;

    public static MissionManager Instance { get; private set; }

    private void Start()
    {
        ResetAllMissions();
        ShowCurrentMission();
    }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Desuscribirse del evento
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameObject.scene.name) // Si cambi� de escena
        {
            ResetAllMissions();
        }
    }

    // Este m�todo se llama para registrar una muerte
    public void RegisterKill(string tag, string name, string tipo)
    {
        if (currentMissionIndex >= baseMissions.Count)
        {
            Debug.LogWarning("No hay m�s misiones.");
            return;
        }

        var currentMission = baseMissions[currentMissionIndex];
        currentMission.RegisterKill(tag, name, tipo);

        if (currentMission.IsCompleted)
        {
            Debug.Log($"�Misi�n completada!: {currentMission.missionName}");

            abilitySelectorUI.SetActive(true);
            currentMissionIndex++;

            if (currentMissionIndex < baseMissions.Count)
                ShowCurrentMission();
            else
                StartCoroutine(ChangeSceneAfterDelay());
        }
        else
        {
            ShowCurrentMission();
        }
    }

    // Muestra la misi�n actual en el HUD
    private void ShowCurrentMission()
    {
        if (currentMissionIndex >= baseMissions.Count) return;

        var currentMission = baseMissions[currentMissionIndex];

        string progress = string.Join(" | ", currentMission.killConditions.Select(k =>
            $"{k.currentAmount}/{k.requiredAmount}"));

        string message = $"{currentMission.missionName}\n{progress}";

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowMission(message);
        }
        else
        {
            Debug.LogWarning("HUDManager no encontrado.");
        }
    }

    // Reinicia todas las misiones
    public void ResetAllMissions()
    {
        foreach (var mission in baseMissions)
        {
            mission.ResetProgress();
        }

        currentMissionIndex = 0;
    }

    // Cambia de escena despu�s de un retraso
    private IEnumerator ChangeSceneAfterDelay()
    {
        float timeout = 10f; // Tiempo m�ximo de espera
        float elapsedTime = 0f;

        while (abilitySelectorUI.activeSelf && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (elapsedTime >= timeout)
        {
            Debug.LogWarning("Timeout alcanzado. Cambiando de escena de todos modos.");
        }
        // Luego cambiar de escena
        SceneManager.LoadScene(nextSceneName);
    }
}
