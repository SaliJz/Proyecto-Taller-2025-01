using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    private enum MissionMode { Purgador, JSS, ElUnico }

    [Header("Misi�n base")]
    [SerializeField] private List<Mission> baseMissions;

    [SerializeField] private GameObject[] teleporters;

    [SerializeField] private float missionStartDelay = 5f;

    [SerializeField] private GameObject[] safeZones;
    [SerializeField] private MissionMode baseMissionMode;
    [SerializeField] private bool useBaseMissionMode = false;

    private bool activeMission = false;

    //private int currentMissionIndex = 0;
    private int currentSafeZoneIndex = 0;

    private MissionMode currentMode;
    private EnemySpawner spawner;
    public static MissionManager Instance { get; private set; }
    public bool ActiveMission => activeMission;

    // Variables para la misi�n "El �nico"
    private float currentCaptureTime;
    private bool isPlayerInZone;
    private bool isEnemyInZone;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        spawner = FindObjectOfType<EnemySpawner>();
    }

    private void Start()
    {
        BeginMission();
    }

    private void Update()
    {
        // Solo ejecutar el Update si la misi�n activa es "El �nico"
        if (activeMission && currentMode == MissionMode.ElUnico)
        {
            HandleElUnicoMission();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BeginMission()
    {
        currentMode = SelectModeByLevel(); // Decide qu� misi�n toca
        StartCoroutine(BeginMissionCoroutine());
    }

    private IEnumerator BeginMissionCoroutine()
    {
        // --- 1. L�gica de Dificultad Procedural ---
        // Se calcula la dificultad basada en el �ndice del GameManager
        int difficultyStep = 0;
        if (GameManager.Instance != null)
        {
            // El �ndice 0 y 1 son tutoriales, as� que el aumento empieza en el �ndice 2
            difficultyStep = Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1);
        }

        if (baseMissions.Count == 0)
        {
            Debug.LogError("No se ha asignado ninguna misi�n en baseMissions.");
            yield break;
        }

        // --- 2. C�lculo de Par�metros de Misi�n ---
        var currentMissionSO = baseMissions[0]; // Coge el SO correspondiente al modo

        // Purgador
        int purgadorKills = 5 + (difficultyStep * 5);
        currentMissionSO.killConditions[0].requiredAmount = purgadorKills;
        currentMissionSO.ResetProgress();

        // El �nico
        float elUnicoSurvivalTime = 120f + (difficultyStep * 30f);
        float elUnicoInitialTime = 30f;

        // JSS
        int jssSimultaneousEnemies = 5 + (difficultyStep * 5);
        float jssDuration = 30f + (difficultyStep * 20f);

        // --- 3. Inicio de Misi�n ---
        float delay = missionStartDelay;
        while (delay > 0f)
        {
            HUDManager.Instance?.ShowMission($"Misi�n comienza en: {Mathf.Ceil(delay)}s");
            delay -= Time.deltaTime;
            yield return null;
        }

        activeMission = true;

        switch (currentMode)
        {
            case MissionMode.Purgador:
                HUDManager.Instance?.ShowMission($"Objetivo: Elimina enemigos\nProgreso: {currentMissionSO.killConditions[0].currentAmount}/{purgadorKills}");
                spawner?.StartPurgeSpawning(purgadorKills, 15, 2f); // Spawnea los enemigos necesarios
                break;

            case MissionMode.JSS:
                HUDManager.Instance?.ShowMission($"Objetivo: Sobrevive\nTiempo restante: {Mathf.Ceil(jssDuration)}s", true);
                spawner?.StartContinuousSpawning(jssSimultaneousEnemies, 1f, jssDuration);
                StartCoroutine(TimerCoroutine(jssDuration)); // Inicia el temporizador de supervivencia
                break;

            case MissionMode.ElUnico:
                HUDManager.Instance?.ShowMission("Objetivo: Captura la zona segura\nPermanece solo para aumentar el tiempo");
                currentCaptureTime = elUnicoInitialTime; // El "colch�n" de tiempo que tiene el jugador
                SelectSafeZone();
                spawner?.StartContinuousSpawning(10, 3f); // Spawn infinito de enemigos a un ritmo moderado
                break;
        }
    }

    // --- L�gica de Misiones Espec�ficas ---

    public void RegisterKill(string tag, string name, string tipo)
    {
        if (!activeMission || currentMode != MissionMode.Purgador) return;

        var currentMissionSO = baseMissions[(int)currentMode];
        currentMissionSO.RegisterKill(tag, name, tipo);

        if (currentMissionSO.IsCompleted)
        {
            CompleteMission();
        }
        else
        {
            string progress = $"{currentMissionSO.killConditions[0].currentAmount}/{currentMissionSO.killConditions[0].requiredAmount}";
            HUDManager.Instance?.ShowMission($"Objetivo: Elimina enemigos\nProgreso: \n{progress}");
        }
    }

    private void HandleElUnicoMission()
    {
        if (isPlayerInZone && !isEnemyInZone)
        {
            currentCaptureTime += Time.deltaTime; // El tiempo sube si el jugador est� solo en la zona
        }
        else if (!isPlayerInZone)
        {
            currentCaptureTime -= Time.deltaTime; // El tiempo baja si el jugador est� fuera
        }
        // Si el jugador y el enemigo est�n en la zona, el tiempo se congela.

        float totalTime = 120f + (Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1) * 30f);
        HUDManager.Instance?.UpdateMissionProgress(currentCaptureTime, totalTime, false);
        HUDManager.Instance?.UpdateTimer(currentCaptureTime);

        if (currentCaptureTime >= totalTime) CompleteMission();
        if (currentCaptureTime <= 0) FailedMission();
    }

    // --- Flujo de Final de Misi�n ---

    public void CompleteMission()
    {
        if (!activeMission) return;
        activeMission = false;

        spawner?.StopAndClearSpawner();

        // Desactivar zonas, etc. si es necesario
        if (currentMode == MissionMode.ElUnico)
        {
            safeZones[currentSafeZoneIndex].GetComponent<CaptureZone>()?.Deactivate();
        }

        StartCoroutine(MissionCompleteSequence());
    }

    private IEnumerator MissionCompleteSequence()
    {
        // 1. Mostrar mensaje de Misi�n Completa
        HUDManager.Instance?.ShowMission("�Misi�n Completa!");
        yield return new WaitForSeconds(2f); // Pausa breve

        // 2. Iniciar cuenta atr�s para la tienda
        float countdown = 5f;
        while (countdown > 0)
        {
            HUDManager.Instance?.ShowMission($"La tienda se abrir� en {Mathf.Ceil(countdown)}...");
            countdown -= Time.deltaTime;
            yield return null;
        }

        // 3. Llamar al GameManager para que maneje la transici�n a la tienda
        GameManager.Instance?.OnLevelCompleted();
    }

    private void FailedMission()
    {
        activeMission = false;

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1000, transform.position);
        }
    }

    private IEnumerator TimerCoroutine(float duration)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            HUDManager.Instance?.ShowMission($"Objetivo: Sobrevive\nTiempo restante: <b><color=\"red\">{Mathf.Ceil(remaining)} s </b></color=\"red\">", true);
            //HUDManager.Instance.UpdateTimer(remaining);
            yield return null;
        }

        CompleteMission();
    }

    // --- M�todos de Ayuda ---
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

    public void SetActiveCapture(bool isActive) { isPlayerInZone = isActive; }
    public void SetEnemyOnCaptureZone(bool isEnemy) { isEnemyInZone = isEnemy; }
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
}