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
    private enum MissionMode { Purgador, JSS }

    [Header("Misi�n base")]
    [SerializeField] private List<Mission> baseMissions; // ScriptableObjects
    [SerializeField] private GameObject abilitySelectorUI;
    [SerializeField] private string nextSceneName = "VictoryScene";

    [Header("Configuraci�n de misi�n")]
    [SerializeField] private bool canChangeScene = true; // Para evitar que cambie de escena al completar la misi�n
    [SerializeField] private float missionStartDelay = 10f; // Retraso antes de iniciar la misi�n
    [SerializeField] private int jssMaxEnemiesInTotal = -1; // Sin l�mite de enemigos en escena
    [SerializeField] private float jssMissionInterval = 1f; // Intervalo de misi�n JSS

    private int currentMissionIndex = 0;
    private MissionMode currentMode;
    private EnemySpawner spawner;
    private Coroutine delayRoutine;

    public static MissionManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento

    }

    private void Start()
    {
        ResetAllMissions();
        BeginMission();
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
            Log("No hay m�s misiones.");
            return;
        }

        var currentMission = baseMissions[currentMissionIndex];
        currentMission.RegisterKill(tag, name, tipo);

        string progress = string.Join(" | ", currentMission.killConditions.Select(k =>
            $"{k.currentAmount}/{k.requiredAmount}")); // Progreso de cada condici�n

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

    // Comienza la misi�n actual 
    private void BeginMission()
    {
        Log($"Comenzando misi�n: {baseMissions[currentMissionIndex].missionName}");

        currentMode = SelectModeByLevel();
        Log($"Modo de misi�n: {currentMode}");

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
            spawner.ResetSpawner(); // reiniciar spawner
            spawner.JssSpawnCondition(jssMaxEnemiesInTotal, jssMissionInterval);
            int simult = currentMission.killConditions[0].requiredAmount;
            spawner.SpawnWave(simult);
            StartCoroutine(JSSCoroutine(currentMission.duration));
            HUDManager.Instance?.ShowMission(message, true); // Mostrar misi�n JSS
        }
        else
        {
            HUDManager.Instance?.ShowMission(message); // Mostrar misi�n Purgador
        }
    }

    private MissionMode SelectModeByLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int lvl;

        // Extrae un n�mero al final del nombre
        // Busca d�gitos consecutivos al final
        Match match = Regex.Match(sceneName, @"(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out lvl))
        {
            if (lvl <= 2) return MissionMode.Purgador;
            else if (lvl == 3) return MissionMode.JSS;
            else Log($"Nivel {lvl} detectado. Selecci�n aleatoria.");
        }
        else
        {
            // nombre no coincide con NivelX: elegimos al azar
            Log($"Nombre de escena '{sceneName}' no tiene nivel. Modo al azar.");
        }

        // Si no se detecta n�mero, o es 4 o m�s, selecciona aleatoriamente
        MissionMode randomMode = UnityEngine.Random.value < 0.5f
            ? MissionMode.Purgador
            : MissionMode.JSS;

        Log($"Modo aleatorio seleccionado: {randomMode}");

        return randomMode;
    }

    private IEnumerator JSSCoroutine(float duration)
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
            OnJSSFailed();
            Log("�Tiempo agotado! Misi�n JSS fallida.");
        }
    }

    private void OnJSSFailed()
    {
        Log("�Tiempo agotado! Misi�n JSS fallida.");
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1000); // O el da�o que desees
        }
        else
        {
            Log("PlayerHealth no encontrado.");
        }
    }

    private void CompleteMission()
    {
        Log($"�Misi�n completada!: {baseMissions[currentMissionIndex].missionName}");
        abilitySelectorUI.SetActive(true);
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
                Log("No se puede cambiar de escena.");
                return; // No cambiar de escena
            }
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

        while (abilitySelectorUI.activeSelf && elapsedTime < timeout)
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
