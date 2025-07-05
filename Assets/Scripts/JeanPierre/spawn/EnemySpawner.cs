using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private float spawnRadius = 2f;
    private int maxEnemiesInScene = 20; // Este valor ahora será controlado por la misión

    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    private Coroutine currentSpawnRoutine;

    // Detiene cualquier rutina de spawn anterior y limpia los enemigos
    public void StopAndClearSpawner()
    {
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
        }
        activeEnemies.RemoveWhere(e => e == null);
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();
    }

    // Inicia una rutina para generar enemigos hasta un límite total (Purgador)
    public void StartPurgeSpawning(int totalToSpawn, int maxOnScreen, float interval)
    {
        StopAndClearSpawner();
        this.maxEnemiesInScene = maxOnScreen;
        currentSpawnRoutine = StartCoroutine(SpawnUntilCountRoutine(totalToSpawn, interval));
    }

    // Inicia una rutina de spawn continuo por un tiempo determinado (JSS y El Único)
    public void StartContinuousSpawning(int maxOnScreen, float interval, float duration = -1f)
    {
        StopAndClearSpawner();
        this.maxEnemiesInScene = maxOnScreen;
        currentSpawnRoutine = StartCoroutine(SpawnContinuouslyRoutine(interval, duration));
    }

    private IEnumerator SpawnUntilCountRoutine(int totalToSpawn, float interval)
    {
        int spawnedCount = 0;
        while (spawnedCount < totalToSpawn)
        {
            yield return new WaitForSeconds(interval);

            if (!MissionManager.Instance?.ActiveMission ?? true) continue;

            if (CanSpawn())
            {
                SpawnSingleEnemy();
                spawnedCount++;
            }
        }
    }

    private IEnumerator SpawnContinuouslyRoutine(float interval, float duration)
    {
        float timer = 0f;
        // Si la duración es negativa, el bucle es infinito (para "El Único")
        while (duration < 0 || timer < duration)
        {
            yield return new WaitForSeconds(interval);

            if (!MissionManager.Instance?.ActiveMission ?? true) continue;

            if (CanSpawn())
            {
                SpawnSingleEnemy();
            }

            if (duration > 0)
            {
                timer += interval;
            }
        }
    }

    private bool CanSpawn()
    {
        activeEnemies.RemoveWhere(e => e == null); // Limpieza de enemigos muertos
        return activeEnemies.Count < maxEnemiesInScene;
    }

    private void SpawnSingleEnemy()
    {
        var availablePoints = spawnPoints.Where(sp => sp.IsAvailable).ToList();
        if (availablePoints.Count == 0) return;

        GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        SpawnPoint randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = randomPoint.transform.position + new Vector3(offset.x, 0, offset.y);

        GameObject enemy = Instantiate(randomEnemyPrefab, spawnPosition, randomPoint.transform.rotation);
        activeEnemies.Add(enemy);
    }

    public void ResetSpawner()
    {
        StopAndClearSpawner();
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
        }
        currentSpawnRoutine = null;
    }
}









//using UnityEngine;
//using System.Collections;
//using System.Linq;
//using System.Collections.Generic;

//public class EnemySpawner : MonoBehaviour
//{
//    [Header("Configuración de Spawn")]
//    public GameObject[] enemyPrefabs;       // Prefabs de enemigos
//    public float spawnInterval = 3f;        // Tiempo entre oleadas
//    public float spawnRadius = 2f;          // Radio de dispersión alrededor del punto

//    [Header("Progresión de dificultad")]
//    public float levelDuration = 60f;       // Tiempo (segundos) para aumentar nivel
//    private float levelTimer = 0f;
//    private int currentLevel = 1;           // Nivel 1 => 2 enemigos únicos
//    private int maxLevel = 3;               // Nivel 3 => 4 enemigos únicos

//    [SerializeField] private int maxEnemiesInScene = 20;
//    [SerializeField] private int maxEnemiesInTotal = 50;
//    [SerializeField] private SpawnPoint[] spawnPoints;

//    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
//    private int totalEnemiesSpawned = 0;

//    private void Start()
//    {
//        if (enemyPrefabs.Length < 4 || spawnPoints.Length == 0)
//        {
//            Debug.Log("¡Se requieren al menos 4 prefabs de enemigos y puntos de spawn!");
//            return;
//        }
//        Debug.Log("Iniciando Spawn");
//    }

//    private void OnEnable()
//    {
//        levelTimer = 0f;
//        currentLevel = 1;
//        StartCoroutine(SpawnRoutine());
//    }

//    private void Update()
//    {
//        levelTimer += Time.deltaTime;
//        if (levelTimer >= levelDuration && currentLevel < maxLevel)
//        {
//            levelTimer -= levelDuration;
//            currentLevel++;
//            Debug.Log($"Nivel aumentado a {currentLevel} => {currentLevel + 1} enemigos distintos");
//        }
//    }

//    private IEnumerator SpawnRoutine()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(spawnInterval);
//            CleanUpInactiveEnemies();
//            if (ShouldStopSpawning()) continue;
//            SpawnWaveUnique();
//        }
//    }

//    private void CleanUpInactiveEnemies() => activeEnemies.RemoveWhere(e => e == null);

//    private bool ShouldStopSpawning()
//    {
//        if (activeEnemies.Count >= maxEnemiesInScene)
//        {
//            Debug.Log("Límite de enemigos en escena alcanzado.");
//            return true;
//        }
//        if (maxEnemiesInTotal >= 0 && totalEnemiesSpawned >= maxEnemiesInTotal)
//        {
//            Debug.Log("Límite global de enemigos alcanzado.");
//            return true;
//        }
//        return false;
//    }

//    // Nuevo método: elige N índices únicos basados en currentLevel
//    private void SpawnWaveUnique()
//    {
//        int uniqueCount = Mathf.Clamp(currentLevel + 1, 2, 4);
//        // Preparar índices únicos de prefab
//        var indices = Enumerable.Range(0, enemyPrefabs.Length)
//                                .OrderBy(x => Random.value)
//                                .Take(uniqueCount)
//                                .ToList();

//        foreach (int idx in indices)
//        {
//            if (ShouldStopSpawning()) break;
//            SpawnEnemyAtIndex(idx);
//        }
//    }

//    private void SpawnEnemyAtIndex(int prefabIndex)
//    {
//        var points = spawnPoints.Where(sp => sp.IsAvailable).ToList();
//        if (points.Count == 0) return;

//        var point = points[Random.Range(0, points.Count)];
//        Vector2 offset = Random.insideUnitCircle * spawnRadius;
//        Vector3 pos = point.transform.position + new Vector3(offset.x, 0, offset.y);

//        var enemy = Instantiate(enemyPrefabs[prefabIndex], pos, point.transform.rotation);
//        activeEnemies.Add(enemy);
//        totalEnemiesSpawned++;
//        Debug.Log($"Enemigo {totalEnemiesSpawned} instanciado (prefab {prefabIndex}).");
//    }

//    // Compatibilidad con MissionManager
//    public void EnemiesKilledCount(int count)
//    {
//        totalEnemiesSpawned = Mathf.Max(0, totalEnemiesSpawned - count);
//    }

//    public void SpawnCondition(int maxTotal, float interval)
//    {
//        maxEnemiesInTotal = maxTotal;
//        spawnInterval = interval;
//    }

//    public void SpawnWave(int totalEnemies)
//    {
//        // Mantener vieja funcionalidad si se llama explícitamente
//        StartCoroutine(CustomWave(totalEnemies));
//    }

//    private IEnumerator CustomWave(int totalEnemies)
//    {
//        int spawned = 0;
//        while (spawned < totalEnemies && !ShouldStopSpawning())
//        {
//            SpawnWaveUnique();
//            spawned += Mathf.Clamp(currentLevel + 1, 2, 4);
//            yield return new WaitForSeconds(spawnInterval);
//        }
//    }

//    public void ResetSpawner()
//    {
//        totalEnemiesSpawned = 0;
//        activeEnemies.Clear();
//        StopAllCoroutines();
//        OnEnable();
//    }

//    private void OnDrawGizmosSelected()
//    {
//        if (spawnPoints == null) return;
//        Gizmos.color = Color.red;
//        foreach (var p in spawnPoints)
//        {
//            Gizmos.DrawWireSphere(p.transform.position, spawnRadius);
//            Gizmos.color = p.IsAvailable ? Color.green : Color.gray;
//            Gizmos.DrawSphere(p.transform.position, 0.2f);
//        }
//    }
//}























//using UnityEngine;
//using System.Collections;
//using System.Linq;
//using System.Collections.Generic;

//public class EnemySpawner : MonoBehaviour
//{
//    [Header("Configuración de Spawn")]
//    public GameObject[] enemyPrefabs;       // Prefabs de enemigos
//    public float spawnInterval = 3f;        // Tiempo entre oleadas
//    public int enemiesPerSpawn = 5;         // Enemigos a crear por intervalo
//    public float spawnRadius = 2f;          // Radio de dispersión alrededor del punto

//    [SerializeField] private int maxEnemiesInScene = 20; // Máximo de enemigos en la escena
//    [SerializeField] private int maxEnemiesInTotal = 50; // Máximo de enemigos totales 
//    [SerializeField] private SpawnPoint[] spawnPoints; // Lista de puntos con disponibilidad

//    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
//    private int totalEnemiesSpawned = 0; // Contador de enemigos totales creados

//    private void Start()
//    {
//        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
//        {
//            Debug.Log("¡Faltan prefabs de enemigos o puntos de spawn!");
//            return;
//        }

//        Debug.Log("Iniciando Spawn");
//    }

//    private void OnEnable()
//    {
//        StartCoroutine(SpawnRoutine());
//    }

//    public void SpawnCondition(int maxEnemiesInTotal, float spawnInterval)
//    {
//        this.maxEnemiesInTotal = maxEnemiesInTotal;
//        this.spawnInterval = spawnInterval;
//    }

//    public void EnemiesKilledCount(int count)
//    {
//        totalEnemiesSpawned -= count;
//        if (totalEnemiesSpawned < 0) totalEnemiesSpawned = 0; // Evitar números negativos
//    }

//    private IEnumerator SpawnRoutine()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(spawnInterval);

//            CleanUpInactiveEnemies();

//            if (ShouldStopSpawning()) yield break;

//            SpawnEnemies();
//        }
//    }

//    private void CleanUpInactiveEnemies()
//    {
//        activeEnemies.RemoveWhere(e => e == null);
//    }

//    private bool ShouldStopSpawning()
//    {
//        if (activeEnemies.Count >= maxEnemiesInScene)
//        {
//            Debug.Log("Límite de enemigos alcanzado. Esperando espacio libre...");
//            return true; // Detener el spawn temporalmente
//        }

//        if (maxEnemiesInTotal >= 0 && totalEnemiesSpawned >= maxEnemiesInTotal)
//        {
//            Debug.Log("Límite global de enemigos alcanzado.");
//            return true; // Detener el spawn
//        }

//        return false; // Continuar spawn
//    }

//    private void SpawnEnemies()
//    {
//        int availableByTotal = (maxEnemiesInTotal >= 0) ? (maxEnemiesInTotal - totalEnemiesSpawned) : enemiesPerSpawn;
//        int availableSpawnSlots = Mathf.Min(
//        enemiesPerSpawn,
//        maxEnemiesInScene - activeEnemies.Count,
//        availableByTotal
//        );

//        if (availableSpawnSlots <= 0)
//        {
//            Debug.Log("No hay espacio para más enemigos.");
//            return; // No hay espacio para más enemigos
//        }

//        for (int i = 0; i < availableSpawnSlots; i++)
//        {
//            var enemy = SpawnSingleEnemy();
//            if (enemy != null)
//            {
//                totalEnemiesSpawned++;
//                Debug.Log($"Enemigo {totalEnemiesSpawned} instanciado.");
//            }
//        }
//    }

//    public void SpawnWave(int totalEnemies)
//    {
//        int spawnerCount = Mathf.Min(spawnPoints.Length, 5);
//        int perSpawner = totalEnemies / spawnerCount;
//        int extra = totalEnemies % spawnerCount;
//        int spawned = 0;

//        for (int i = 0; i < spawnerCount; i++)
//        {
//            int toSpawn = perSpawner + (i < extra ? 1 : 0);
//            for (int j = 0; j < toSpawn; j++)
//            {
//                var enemy = SpawnSingleEnemy();
//                if (enemy != null)
//                {
//                    totalEnemiesSpawned++;
//                    spawned++;
//                    Debug.Log($"Enemigo {totalEnemiesSpawned} instanciado.");
//                }
//            }
//        }

//        if (spawned < totalEnemies)
//        {
//            Debug.Log($"Solo se pudieron instanciar {spawned} de {totalEnemies} enemigos solicitados.");
//        }
//    }

//    private GameObject SpawnSingleEnemy()
//    {
//        // Filtrar solo los puntos habilitados
//        var availablePoints = spawnPoints.Where(spawnPoint => spawnPoint.IsAvailable).ToList();

//        if (availablePoints.Count == 0)
//        {
//            return null; // No hay puntos disponibles
//        }

//        GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
//        SpawnPoint randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];

//        Vector2 offset = Random.insideUnitCircle * spawnRadius;
//        Vector3 spawnPosition = randomPoint.transform.position +
//                                new Vector3(offset.x, 0, offset.y);

//        GameObject enemy = Instantiate(randomEnemy, spawnPosition, randomPoint.transform.rotation); // Instanciar enemigo

//        activeEnemies.Add(enemy); // Agregado a la lista de enemigos activos
//        return enemy;
//    }

//    public void ResetSpawner()
//    {
//        totalEnemiesSpawned = 0;
//        activeEnemies.Clear();
//        StopAllCoroutines(); // Por si acaso
//        StartCoroutine(SpawnRoutine()); // Reinicia el spawn
//    }

//    // Visualizar el radio de spawn en el editor
//    private void OnDrawGizmosSelected()
//    {
//        if (spawnPoints == null) return;

//        Gizmos.color = Color.red;
//        foreach (var point in spawnPoints)
//        {
//            Gizmos.DrawWireSphere(point.transform.position, spawnRadius);
//            Gizmos.color = point.IsAvailable ? Color.green : Color.gray;
//            Gizmos.DrawSphere(point.transform.position, 0.2f);
//        }
//    }
//}