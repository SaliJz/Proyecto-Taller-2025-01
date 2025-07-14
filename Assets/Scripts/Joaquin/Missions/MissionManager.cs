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

    private int currentSafeZoneIndex = 0;

    private MissionMode currentMode;
    private EnemySpawner spawner;
    public static MissionManager Instance { get; private set; }
    public bool ActiveMission => activeMission;

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
        currentMode = SelectModeByLevel();
        StartCoroutine(BeginMissionCoroutine());
    }

    private IEnumerator BeginMissionCoroutine()
    {
        int difficultyStep = 0;
        if (GameManager.Instance != null)
        {
            difficultyStep = Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1);
        }

        if (baseMissions.Count == 0)
        {
            Debug.LogError("No se ha asignado ninguna misión en baseMissions.");
            yield break;
        }

        var currentMissionSO = baseMissions[0];

        int purgadorKills = 5 + (difficultyStep * 5);
        currentMissionSO.killConditions[0].requiredAmount = purgadorKills;
        currentMissionSO.ResetProgress();

        float elUnicoSurvivalTime = 120f + (difficultyStep * 30f);
        float elUnicoInitialTime = 30f;

        int jssSimultaneousEnemies = 5 + (difficultyStep * 5);
        float jssDuration = 30f + (difficultyStep * 20f);

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
                spawner?.StartPurgeSpawning(purgadorKills, 15, 2f);
                break;

            case MissionMode.JSS:
                HUDManager.Instance?.ShowMission($"Objetivo: Sobrevive\nTiempo restante: {Mathf.Ceil(jssDuration)}s", true);
                spawner?.StartContinuousSpawning(jssSimultaneousEnemies, 1f, jssDuration);
                StartCoroutine(TimerCoroutine(jssDuration));
                break;

            case MissionMode.ElUnico:
                HUDManager.Instance?.ShowMission("Objetivo: Captura la zona segura\nPermanece solo para aumentar el tiempo");
                currentCaptureTime = elUnicoInitialTime;
                SelectSafeZone();
                spawner?.StartContinuousSpawning(10, 3f);
                break;
        }
    }

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
            currentCaptureTime += Time.deltaTime;
        }
        else if (!isPlayerInZone)
        {
            currentCaptureTime -= Time.deltaTime;
        }

        float totalTime = 120f + (Mathf.Max(0, GameManager.Instance.CurrentLevelIndex - 1) * 30f);
        HUDManager.Instance?.UpdateMissionProgress(currentCaptureTime, totalTime, false);
        HUDManager.Instance?.UpdateTimer(currentCaptureTime);

        if (currentCaptureTime >= totalTime) CompleteMission();
        if (currentCaptureTime <= 0) FailedMission();
    }

    public void CompleteMission()
    {
        if (!activeMission) return;
        activeMission = false;

        spawner?.StopAndClearSpawner();

        if (currentMode == MissionMode.ElUnico)
        {
            safeZones[currentSafeZoneIndex].GetComponent<CaptureZone>()?.Deactivate();
        }

        StartCoroutine(MissionCompleteSequence());
    }

    private IEnumerator MissionCompleteSequence()
    {
        HUDManager.Instance?.ShowMission("¡Misión Completa!");
        yield return new WaitForSeconds(2f);

        float countdown = 5f;
        while (countdown > 0)
        {
            HUDManager.Instance?.ShowMission($"La tienda se abrirá en {Mathf.Ceil(countdown)}...");
            countdown -= Time.deltaTime;
            yield return null;
        }

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

    private MissionMode SelectModeByLevel()
    {
        if (useBaseMissionMode)
        {
            if (baseMissionMode == MissionMode.Purgador || baseMissionMode == MissionMode.ElUnico || baseMissionMode == MissionMode.JSS)
            {
                return baseMissionMode;
            }
        }
        else
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int lvl;

            Match match = Regex.Match(sceneName, @"(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out lvl))
            {
                if (lvl == 1) return MissionMode.Purgador;
                else if (lvl == 2) return MissionMode.ElUnico;
                else if (lvl == 3) return MissionMode.JSS;
            }

            return (MissionMode)UnityEngine.Random.Range(0, 3);
        }

        return MissionMode.Purgador;
    }

    public void SetActiveCapture(bool isActive) { isPlayerInZone = isActive; }
    public void SetEnemyOnCaptureZone(bool isEnemy) { isEnemyInZone = isEnemy; }
    private void SelectSafeZone()
    {
        if (safeZones == null || safeZones.Length == 0) return;

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