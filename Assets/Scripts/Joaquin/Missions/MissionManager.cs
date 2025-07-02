using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    private enum MissionMode { Purgador, JSS, ElUnico }

    [Header("Misi�n base")]
    [SerializeField] private List<Mission> baseMissions;

    [SerializeField] private GameObject[] teleporters;

    [SerializeField] private string nextSceneName = "VictoryScene";

    [Header("Configuraci�n de misi�n")]
    [SerializeField] private bool canChangeScene = true;

    [SerializeField] private float missionStartDelay = 10f;
    [SerializeField] private int maxEnemiesInTotal = -1;
    [SerializeField] private float spawnInterval = 1f;

    [SerializeField] private GameObject[] safeZones;
    [SerializeField] private float totalCaptureTime = 120f;
    [SerializeField] private float currentTimeCapture = 30f;
    [SerializeField] private MissionMode baseMissionMode;
    [SerializeField] private bool useBaseMissionMode = false;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();

            if (spawner == null)
            {
                Debug.LogError("No se encontr� un EnemySpawner en la escena.");
            }
        }
    }

    private void OnEnable()
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
                return;
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
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterKill(string tag, string name, string tipo)
    {
        if (currentMissionIndex >= baseMissions.Count)
        {
            return;
        }

        var currentMission = baseMissions[currentMissionIndex];
        currentMission.RegisterKill(tag, name, tipo);

        string progress = string.Join(" | ", currentMission.killConditions.Select(k =>
            $"{k.currentAmount}/{k.requiredAmount}"));

        string message = $"{currentMission.missionName}\n{progress}";

        if (spawner != null)
        {
            spawner.EnemiesKilledCount(1);
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
        }
    }

    // Comienza la misi�n actual 
    private void BeginMission()
    {
        currentMode = SelectModeByLevel();

        if (currentMissionIndex >= baseMissions.Count) return;

        if (delayRoutine != null) StopCoroutine(delayRoutine);
        delayRoutine = StartCoroutine(BeginMissionAfterDelay());
    }

    private IEnumerator BeginMissionAfterDelay()
    {
        float remaining = missionStartDelay;
        while (remaining > 0f)
        {
            HUDManager.Instance?.ShowMission($"Misi�n comienza en: {Mathf.Ceil(remaining)}s");
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
            maxEnemiesInTotal = 5;
            spawnInterval= 2.5f;
            spawner.SpawnCondition(maxEnemiesInTotal, spawnInterval);
            spawner.SpawnWave(simult);

            StartCoroutine(TimerCoroutine(currentMission.duration));

            HUDManager.Instance?.ShowMission(message, true); // Mostrar misi�n JSS
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
            HUDManager.Instance?.ShowMission(message); // Mostrar misi�n Purgador
        }
    }

    private MissionMode SelectModeByLevel()
    {
        if (useBaseMissionMode)
        {
            if (baseMissionMode == MissionMode.Purgador || baseMissionMode == MissionMode.ElUnico || baseMissionMode == MissionMode.JSS)
            {
                return baseMissionMode; // Si se ha configurado un modo base, usarlo
            }
        }
        else
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int lvl;

            // Extrae un n�mero al final del nombre
            // Busca d�gitos consecutivos al final
            Match match = Regex.Match(sceneName, @"(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out lvl))
            {
                if (lvl == 1) return MissionMode.Purgador;
                else if (lvl == 2) return MissionMode.ElUnico;
                else if (lvl == 3) return MissionMode.JSS;
            }

            // Si no se detecta n�mero, o es 4 o m�s, selecciona aleatoriamente
            return (MissionMode)UnityEngine.Random.Range(0, 3);/* (MissionMode)UnityEngine.Random.Range(0, 3);*/
        }

        return MissionMode.Purgador; // Por defecto, si no se ha configurado nada
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
            HUDManager.Instance?.ShowMission("�Enemigos en la zona de captura!", true);
        }
        else
        {
            HUDManager.Instance?.ShowMission("Zona segura despejada. Contin�a capturando.", true);
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
            selectedZone.SetActive(true);
            CaptureZone captureZone = selectedZone.GetComponent<CaptureZone>();
            if (captureZone != null)
            {
                captureZone.Activate();
            }
            HUDManager.Instance?.ShowMission($"Dir�gete a la zona segura: {selectedZone.name}", true);
        }

        activeMission = true;
    }
    /*
    private void SelectTeleporter()
    {
        if (teleporters == null || teleporters.Length == 0) return;
        // Desactiva todos los teleportadores primero
        foreach (var teleporter in teleporters)
        {
            if (teleporter != null) teleporter.SetActive(false);
        }
        int randomIndex = UnityEngine.Random.Range(0, teleporters.Length);
        randomTeleporterIndex = randomIndex; // Guardar el �ndice seleccionado
        GameObject selectedTeleporter = teleporters[randomIndex];
        if (selectedTeleporter != null)
        {
            selectedTeleporter.SetActive(true);
            HUDManager.Instance?.ShowMission($"Dir�gete al teletransportador: {selectedTeleporter.name}", true);
        }
    }
    */
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
        }
    }

    private void FailedMission()
    {
        activeMission = false;

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1000); // O el da�o que desees
        }
    }

    public void CompleteMission() 
    {
        activeMission = false;

        GameManager.Instance?.OnLevelCompleted();

        //electTeleporter();
        //TutorialManager.Instance.StartScenarioByManual(8);

        currentMissionIndex++;

        if (currentMissionIndex < baseMissions.Count)
        {
            BeginMission();
        }
        else
        {
            HUDManager.Instance?.ShowMission("Misi�n completa. Proceda al siguiente nivel.");
            HideMissionTimer(5f);

            if (canChangeScene)
            {
                StartCoroutine(ChangeSceneAfterDelay());
            }
            else
            {
                return;
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

    // Cambia de escena despu�s de un retraso
    private IEnumerator ChangeSceneAfterDelay()
    {
        float timeout = 10f; // Tiempo m�ximo de espera
        float elapsedTime = 0f;

        while (teleporters[randomTeleporterIndex].activeSelf && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (elapsedTime >= timeout)
        {
            Debug.LogWarning("Tiempo de espera agotado para el teletransportador. Cambiando de escena de todos modos.");
        }
        // Luego cambiar de escena
        SceneManager.LoadScene(nextSceneName);
    }
}
