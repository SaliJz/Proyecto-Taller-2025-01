using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    private enum MissionMode { Purgador, JSS, ElUnico }

    [Header("Misión base")]
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

    // Variables para la misión "El Único"
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
        // Solo ejecutar el Update si la misión activa es "El Único"
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
        currentMode = SelectModeByLevel(); // Decide qué misión toca
        StartCoroutine(BeginMissionCoroutine());
    }

    private IEnumerator BeginMissionCoroutine()
    {
        // --- 1. Lógica de Dificultad Procedural ---
        // Se calcula la dificultad basada en el índice del GameManager
        int difficultyStep = 0;
        if (GameManager.Instance != null)
        {
            // El índice 0 y 1 son tutoriales, así que el aumento empieza en el índice 2
            difficultyStep = Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1);
        }

        if (baseMissions.Count == 0)
        {
            Debug.LogError("No se ha asignado ninguna misión en baseMissions.");
            yield break;
        }

        // --- 2. Cálculo de Parámetros de Misión ---
        var currentMissionSO = baseMissions[0]; // Coge el SO correspondiente al modo

        // Purgador
        int purgadorKills = 5 + (difficultyStep * 5);
        currentMissionSO.killConditions[0].requiredAmount = purgadorKills;
        currentMissionSO.ResetProgress();

        // El Único
        float elUnicoSurvivalTime = 120f + (difficultyStep * 30f);
        float elUnicoInitialTime = 30f;

        // JSS
        int jssSimultaneousEnemies = 5 + (difficultyStep * 5);
        float jssDuration = 30f + (difficultyStep * 20f);

        // --- 3. Inicio de Misión ---
        float delay = missionStartDelay;
        while (delay > 0f)
        {
            HUDManager.Instance?.ShowMission($"Misión comienza en: {Mathf.Ceil(delay)}s");
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
                currentCaptureTime = elUnicoInitialTime; // El "colchón" de tiempo que tiene el jugador
                SelectSafeZone();
                spawner?.StartContinuousSpawning(10, 3f); // Spawn infinito de enemigos a un ritmo moderado
                break;
        }
    }

    // --- Lógica de Misiones Específicas ---

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
            currentCaptureTime += Time.deltaTime; // El tiempo sube si el jugador está solo en la zona
        }
        else if (!isPlayerInZone)
        {
            currentCaptureTime -= Time.deltaTime; // El tiempo baja si el jugador está fuera
        }
        // Si el jugador y el enemigo están en la zona, el tiempo se congela.

        float totalTime = 120f + (Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1) * 30f);
        HUDManager.Instance?.UpdateMissionProgress(currentCaptureTime, totalTime, false);
        HUDManager.Instance?.UpdateTimer(currentCaptureTime);

        if (currentCaptureTime >= totalTime) CompleteMission();
        if (currentCaptureTime <= 0) FailedMission();
    }

    // --- Flujo de Final de Misión ---

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
        // 1. Mostrar mensaje de Misión Completa
        HUDManager.Instance?.ShowMission("¡Misión Completa!");
        yield return new WaitForSeconds(2f); // Pausa breve

        // 2. Iniciar cuenta atrás para la tienda
        float countdown = 5f;
        while (countdown > 0)
        {
            HUDManager.Instance?.ShowMission($"La tienda se abrirá en {Mathf.Ceil(countdown)}...");
            countdown -= Time.deltaTime;
            yield return null;
        }

        // 3. Llamar al GameManager para que maneje la transición a la tienda
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

    // --- Métodos de Ayuda ---
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

            // Extrae un número al final del nombre
            // Busca dígitos consecutivos al final
            Match match = Regex.Match(sceneName, @"(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out lvl))
            {
                if (lvl == 1) return MissionMode.Purgador;
                else if (lvl == 2) return MissionMode.ElUnico;
                else if (lvl == 3) return MissionMode.JSS;
            }

            // Si no se detecta número, o es 4 o más, selecciona aleatoriamente
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
            HUDManager.Instance?.ShowMission($"Dirígete a la zona segura: {selectedZone.name}", true);
        }

        activeMission = true;
    }
}