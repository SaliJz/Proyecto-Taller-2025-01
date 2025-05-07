using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    [Header("Misi�n base")]
    [SerializeField] private List<Mission> baseMissions; // ScriptableObjects
    [SerializeField] private GameObject abilitySelectorUI;
    [SerializeField] private string nextSceneName = "VictoryScene";

    private List<MissionInstance> runtimeMissions = new();
    private int currentMissionIndex = 0;

    private void Start()
    {
        // Crear instancias nuevas para que no compartan estado
        foreach (var mission in baseMissions)
        {
            runtimeMissions.Add(new MissionInstance { missionSO = mission });
        }

        ResetAllMissions();
        ShowCurrentMission();
    }

    private void Awake()
    {
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
    public void RegisterKill(string killedTag)
    {
        if (currentMissionIndex >= runtimeMissions.Count)
      {
          Debug.LogWarning("No hay m�s misiones para completar.");
          return;
      }

      var mission = runtimeMissions[currentMissionIndex];
      mission.RegisterKill(killedTag);

      if (mission.IsCompleted)
      {
          Debug.Log($"�Misi�n completada!: {mission.missionSO.missionName}");

          abilitySelectorUI.SetActive(true);
          currentMissionIndex++;

          if (currentMissionIndex < runtimeMissions.Count) ShowCurrentMission();
          else StartCoroutine(ChangeSceneAfterDelay());
      }
      else
      {
          ShowCurrentMission();
      }
    }

    // Muestra la misi�n actual en el HUD
    private void ShowCurrentMission()
    {
        Debug.Log($"Misi�n actual: {currentMissionIndex + 1}/{runtimeMissions.Count}");
      if (currentMissionIndex >= runtimeMissions.Count) return;

      var mission = runtimeMissions[currentMissionIndex];
      string message = $"{mission.missionSO.missionName} ({mission.currentAmount}/{mission.missionSO.targetAmount})";

      if (HUDManager.Instance != null)
      {
          HUDManager.Instance.ShowMission(message);
      }
      else
      {
          Debug.LogWarning("HUDManager.Instance es null. No se puede mostrar la misi�n.");
      }
    }

    // Reinicia todas las misiones
    public void ResetAllMissions()
    {
        foreach (var mission in runtimeMissions)
        {
            mission.ResetProgress(); // Reiniciar instancias reales
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

[System.Serializable]
public class MissionInstance
{
    public Mission missionSO; // Referencia al ScriptableObject
    public int currentAmount = 0;

    public bool IsCompleted => currentAmount >= missionSO.targetAmount;

    public void RegisterKill(string killedTag)
    {
        if (killedTag == missionSO.enemyTag || missionSO.enemyTag == "Any")
        {
            currentAmount++;
        }
    }

    public void ResetProgress()
    {
        currentAmount = 0;
    }
}
