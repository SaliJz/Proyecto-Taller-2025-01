using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuraci�n de Spawn")]
    [SerializeField] public GameObject[] enemyPrefabs;
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private float spawnRadius = 2f;
    private int maxEnemiesInScene = 20; // Este valor ahora ser� controlado por la misi�n

    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    private Coroutine currentSpawnRoutine;

    private GameObject dataBaseManager;

    private void Start()
    {
        dataBaseManager = GameObject.Find("DataBaseManager");
    }
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

    // Inicia una rutina para generar enemigos hasta un l�mite total (Purgador)
    public void StartPurgeSpawning(int totalToSpawn, int maxOnScreen, float interval)
    {
        StopAndClearSpawner();
        this.maxEnemiesInScene = maxOnScreen;
        currentSpawnRoutine = StartCoroutine(SpawnUntilCountRoutine(totalToSpawn, interval));
    }

    // Inicia una rutina de spawn continuo por un tiempo determinado (JSS y El �nico)
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
        // Si la duraci�n es negativa, el bucle es infinito (para "El �nico")
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
        activeEnemies.RemoveWhere(e => e == null); 
        return activeEnemies.Count < maxEnemiesInScene;
    }

    private void SpawnSingleEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        var availablePoints = spawnPoints.Where(sp => sp.IsAvailable).ToList();
        if (availablePoints.Count == 0) return;

       GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)]; 

        GetEnemyTypeToInsertData(randomEnemyPrefab);

        SpawnPoint randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = randomPoint.transform.position + new Vector3(offset.x, 0, offset.y);

            GameObject enemy = Instantiate(randomEnemyPrefab, spawnPosition, randomPoint.transform.rotation);
            activeEnemies.Add(enemy);

       
    }

    void GetEnemyTypeToInsertData(GameObject randomEnemyPrefab)
    {
        EnemyType enemyType = randomEnemyPrefab.GetComponent<EnemyType>();
        if(dataBaseManager != null)
        {
          InsertEnemyController insertEnemyController= dataBaseManager.GetComponent<InsertEnemyController>();
          insertEnemyController.Execute(enemyType);
        }
       
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
