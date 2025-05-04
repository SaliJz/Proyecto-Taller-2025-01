using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    [Header("Misión base")]
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameObject.scene.name) // Si cambió de escena
        {
            ResetAllMissions();
        }
    }

    // Este método se llama para registrar una muerte
    public void RegisterKill(string killedTag)
    {
        if (currentMissionIndex >= runtimeMissions.Count) return;

        var mission = runtimeMissions[currentMissionIndex];
        mission.RegisterKill(killedTag);

        if (mission.IsCompleted)
        {
            Debug.Log($"¡Misión completada!: {mission.missionSO.missionName}");

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

    // Muestra la misión actual en el HUD
    private void ShowCurrentMission()
    {
        Debug.Log($"Misión actual: {currentMissionIndex + 1}/{runtimeMissions.Count}");
        if (currentMissionIndex >= runtimeMissions.Count) return;

        var mission = runtimeMissions[currentMissionIndex];
        string message = $"{mission.missionSO.missionName} ({mission.currentAmount}/{mission.missionSO.targetAmount})";
        HUDManager.Instance.ShowMission(message);
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

    // Cambia de escena después de un retraso
    private IEnumerator ChangeSceneAfterDelay()
    {
        // Opcional: esperar a que el jugador seleccione su habilidad
        yield return new WaitUntil(() => abilitySelectorUI.activeSelf == false);

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
