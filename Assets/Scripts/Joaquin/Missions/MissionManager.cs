using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    private enum MissionMode { Purgador, JSS, ElUnico }

    [Header("Misión base")]
    [SerializeField] private List<Mission> baseMissions; // ScriptableObjects

    [SerializeField] private GameObject[] teleporters;

    [SerializeField] private string nextSceneName = "VictoryScene";

    [Header("Configuración de misión")]
    [SerializeField] private bool canChangeScene = true;

    [SerializeField] private float missionStartDelay = 10f;
    [SerializeField] private int maxEnemiesInTotal = -1;
    [SerializeField] private float spawnInterval = 1f;

    [SerializeField] private GameObject[] safeZones;
    [SerializeField] private float totalCaptureTime = 120f;
    [SerializeField] private float currentTimeCapture = 30f;

    private bool isCapturing = false;
    private bool isEnemyInCaptureZone = false;
    private bool activeMission = false;

    private int currentMissionIndex = 0;
    private int currentSafeZoneIndex = 0;

    private MissionMode currentMode;
    private EnemySpawner spawner;
    private Coroutine delayRoutine;
    private int randomTeleporterIndex = 0;
    public static MissionManager Instance { get; private set; }
    public bool ActiveMission => activeMission;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded; // Suscribe al evento

    }

    private void Start()
    {
        ResetAllMissions();
        BeginMission();
    }

    private void Update()
    {
        if (!activeMission) return;

        if (isCapturing)
        {
            if (isEnemyInCaptureZone)
            {
                return; // No se puede capturar si hay enemigos en la zona
            }
            else
            {
                currentTimeCapture += Time.deltaTime;

                HUDManager.Instance?.UpdateMissionProgress(currentTimeCapture, totalCaptureTime, true);
                HUDManager.Instance?.UpdateTimer(currentTimeCapture);

                if (currentTimeCapture >= totalCaptureTime)
                {
                    currentTimeCapture = totalCaptureTime;
                    CompleteMission();
                }
            }
        }
        else
        {
            currentTimeCapture -= Time.deltaTime;

            HUDManager.Instance?.UpdateMissionProgress(currentTimeCapture, totalCaptureTime, true);
            HUDManager.Instance?.UpdateTimer(currentTimeCapture);

            if (currentTimeCapture < 0f)
            {
                currentTimeCapture = 0f;
                FailedMission(); 
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Desuscribe del evento
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameObject.scene.name) // Si cambió de escena
        {
            ResetAllMissions();
        }
    }

    // Este método se llama para registrar una muerte
    public void RegisterKill(string tag, string name, string tipo)
    {
        if (currentMissionIndex >= baseMissions.Count)
        {
            Log("No hay más misiones.");
            return;
        }

        var currentMission = baseMissions[currentMissionIndex];
        currentMission.RegisterKill(tag, name, tipo);

        string progress = string.Join(" | ", currentMission.killConditions.Select(k =>
            $"{k.currentAmount}/{k.requiredAmount}")); // Progreso de cada condición

        string message = $"{currentMission.missionName}\n{progress}";

        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();

        if (enemySpawner != null)
        {
            enemySpawner.EnemiesKilledCount(1); // Restar un enemigo al contador
        }

        if (currentMission.IsCompleted)
        {
            if (HUDManager.Instance != null)
            {
                if (currentMode == MissionMode.JSS)
                {
                    HUDManager.Instance.ShowMission(message, true);
                }
                else
                {
                    HUDManager.Instance.ShowMission(message);
                }
            }
            else
            {
                Log("HUDManager no encontrado.");
            }

            CompleteMission();
        }
        else
        {
            if (HUDManager.Instance != null)
            {
                if (currentMode == MissionMode.JSS)
                {
                    HUDManager.Instance.ShowMission(message, true);
                }
                else
                {
                    HUDManager.Instance.ShowMission(message);
                }
            }
            else
            {
                Log("HUDManager no encontrado.");
            }
        }
    }

    // Comienza la misión actual 
    private void BeginMission()
    {
        Log($"Comenzando misión: {baseMissions[currentMissionIndex].missionName}");

        currentMode = SelectModeByLevel();
        Log($"Modo de misión: {currentMode}");

        if (currentMissionIndex >= baseMissions.Count) return;

        if (delayRoutine != null) StopCoroutine(delayRoutine);
        delayRoutine = StartCoroutine(BeginMissionAfterDelay());
    }

    private IEnumerator BeginMissionAfterDelay()
    {
        float remaining = missionStartDelay;
        while (remaining > 0f)
        {
            HUDManager.Instance?.ShowMission($"Misión comienza en: {Mathf.Ceil(remaining)}s");
            remaining -= Time.deltaTime;
            yield return null;
        }

        var currentMission = baseMissions[currentMissionIndex];

        string progress = string.Join(" | ", currentMission.killConditions.Select(k =>
            $"{k.currentAmount}/{k.requiredAmount}"));
            string message = $"{currentMission.missionName}\n{progress}";

        if (currentMode == MissionMode.JSS)
        {
            spawner.ResetSpawner();

            int simult = currentMission.killConditions[0].requiredAmount;
            maxEnemiesInTotal = -1;
            spawnInterval= 2.5f;
            spawner.SpawnCondition(maxEnemiesInTotal, spawnInterval);
            spawner.SpawnWave(simult);

            StartCoroutine(TimerCoroutine(currentMission.duration));

            HUDManager.Instance?.ShowMission(message, true); // Mostrar misión JSS
        }
        else if (currentMode == MissionMode.ElUnico)
        {
            spawner.ResetSpawner(); 

            SelectSafeZone();
            currentTimeCapture = 30f;
            totalCaptureTime = currentMission.duration;
            maxEnemiesInTotal = -1;
            spawnInterval = 2.5f;
            spawner.SpawnCondition(maxEnemiesInTotal, spawnInterval);
        }
        else
        {
            spawner.ResetSpawner();
            HUDManager.Instance?.ShowMission(message); // Mostrar misión Purgador
        }
    }

    private MissionMode SelectModeByLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int lvl;

        // Extrae un número al final del nombre
        // Busca dígitos consecutivos al final
        Match match = Regex.Match(sceneName, @"(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out lvl))
        {
            if (lvl == 1) return MissionMode.Purgador;
            else if (lvl == 2) return MissionMode.ElUnico;
            else if (lvl == 3) return MissionMode.JSS;
            else Log($"Nivel {lvl} detectado. Selección aleatoria.");
        }
        else
        {
            // Cuando nombre no coincide con NivelX se elige al azar
            Log($"Nombre de escena '{sceneName}' no tiene nivel. Modo al azar.");
        }

        // Si no se detecta número, o es 4 o más, selecciona aleatoriamente
        MissionMode randomMode = (MissionMode)UnityEngine.Random.Range(0, 3);

        Log($"Modo aleatorio seleccionado: {randomMode}");

        return randomMode;
    }

    public void SetActiveCapture(bool isActive)
    {
        isCapturing = isActive;
        if (isCapturing)
        {
            HUDManager.Instance?.ShowMission("Capturando zona segura...", true);
        }
        else
        {
            HUDManager.Instance?.ShowMission("Regresa a la zona de captura", true);
        }
    }

    public void SetEnemyOnCaptureZone(bool isEnemy)
    {
        isEnemyInCaptureZone = isEnemy;
        if (isEnemy)
        {
            HUDManager.Instance?.ShowMission("¡Enemigos en la zona de captura!", true);
        }
        else
        {
            HUDManager.Instance?.ShowMission("Zona segura despejada. Continúa capturando.", true);
        }
    }

    private void SelectSafeZone()
    {
        if (safeZones == null || safeZones.Length == 0) return;

        // Desactiva todas las zonas primero
        foreach (var zone in safeZones)
        {
            if (zone != null) zone.SetActive(false);
        }

        int previousIndex = currentSafeZoneIndex;
        do
        {
            currentSafeZoneIndex = UnityEngine.Random.Range(0, safeZones.Length);
        }

        while (safeZones.Length > 1 && currentSafeZoneIndex == previousIndex);

        GameObject selectedZone = safeZones[currentSafeZoneIndex];

        if (selectedZone != null)
        {
            Log($"Zona segura seleccionada: {selectedZone.name}");
            selectedZone.SetActive(true);
            CaptureZone captureZone = selectedZone.GetComponent<CaptureZone>();
            if (captureZone != null)
            {
                captureZone.Activate();
            }
            HUDManager.Instance?.ShowMission($"Dirígete a la zona segura: {selectedZone.name}", true);
        }
        else
        {
            Log("Zona segura no encontrada.");
        }

        activeMission = true;
    }

    private void SelectTeleporter()
            {
        if (teleporters == null || teleporters.Length == 0) return;
        // Desactiva todos los teleportadores primero
        foreach (var teleporter in teleporters)
        {
            if (teleporter != null) teleporter.SetActive(false);
        }
        int randomIndex = UnityEngine.Random.Range(0, teleporters.Length);
        randomTeleporterIndex = randomIndex; // Guardar el índice seleccionado
        GameObject selectedTeleporter = teleporters[randomIndex];
        if (selectedTeleporter != null)
        {
            Log($"Teletransportador seleccionado: {selectedTeleporter.name}");
            selectedTeleporter.SetActive(true);
            HUDManager.Instance?.ShowMission($"Dirígete al teletransportador: {selectedTeleporter.name}", true);
        }
        else
        {
            Log("Teletransportador no encontrado.");
        }
    }

    private IEnumerator TimerCoroutine(float duration)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            HUDManager.Instance.UpdateTimer(remaining);
            yield return null;
        }

        if (baseMissions[currentMissionIndex].IsCompleted)
        {
            CompleteMission();
        }
        else
        {
            FailedMission();
            Log("¡Tiempo agotado! Misión JSS fallida.");
        }
    }

    private void FailedMission()
    {
        Log("Misión fallida.");

        activeMission = false;

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1000); // O el daño que desees
        }
        else
        {
            Log("PlayerHealth no encontrado.");
        }
    }

    private void CompleteMission()
    {
        Log($"¡Misión completada!: {baseMissions[currentMissionIndex].missionName}");

        activeMission = false;

        SelectTeleporter();
        currentMissionIndex++;

        if (currentMissionIndex < baseMissions.Count)
        {
            BeginMission();
        }
        else
        {
            HUDManager.Instance?.ShowMission("Misión completa. Proceda al siguiente nivel.");
            HideMissionTimer(5f);

            if (canChangeScene)
            {
                StartCoroutine(ChangeSceneAfterDelay());
            }
            else
            {
                Log("No se puede cambiar de escena.");
                return; // No cambiar de escena
            }
        }

        if (currentMode == MissionMode.ElUnico)
        {
            GameObject selectedZone = safeZones[currentSafeZoneIndex];
            CaptureZone captureZone = selectedZone.GetComponent<CaptureZone>();

            if (captureZone != null)
            {
                captureZone.Deactivate();
            }

            isCapturing = false;
            isEnemyInCaptureZone = false;
        }
    }

    private IEnumerator HideMissionTimer(float time)
    {
        float remaining = time;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;

            yield return null;
        }

        HUDManager.Instance?.HideMission();
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

    // Cambia de escena después de un retraso
    private IEnumerator ChangeSceneAfterDelay()
    {
        float timeout = 10f; // Tiempo máximo de espera
        float elapsedTime = 0f;

        while (teleporters[randomTeleporterIndex].activeSelf && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (elapsedTime >= timeout)
        {
            Log("Timeout alcanzado. Cambiando de escena de todos modos.");
        }
        // Luego cambiar de escena
        SceneManager.LoadScene(nextSceneName);
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log("[MissionManager]" + message);
    }
#endif
}
