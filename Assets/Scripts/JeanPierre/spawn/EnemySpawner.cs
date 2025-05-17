using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public GameObject[] enemyPrefabs;       // Prefabs de enemigos
    public float spawnInterval = 3f;        // Tiempo entre oleadas
    public int enemiesPerSpawn = 5;         // Enemigos a crear por intervalo
    public float spawnRadius = 2f;          // Radio de dispersión alrededor del punto

    [SerializeField] private int maxEnemiesInScene = 20; // Máximo de enemigos en la escena
    [SerializeField] private int maxEnemiesInTotal = 50; // Máximo de enemigos totales 
    [SerializeField] private SpawnPoint[] spawnPoints; // Lista de puntos con disponibilidad

    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    private int totalEnemiesSpawned = 0; // Contador de enemigos totales creados

    private void Start()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Log("¡Faltan prefabs de enemigos o puntos de spawn!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    public void JssSpawnCondition(int maxEnemiesInTotal, float spawnInterval)
    {
        this.maxEnemiesInTotal = maxEnemiesInTotal;
        this.spawnInterval = spawnInterval;
    }

    public void EnemiesKilledCount(int count)
    {
        totalEnemiesSpawned -= count;
        if (totalEnemiesSpawned < 0) totalEnemiesSpawned = 0; // Evitar números negativos
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            CleanUpInactiveEnemies();

            if (ShouldStopSpawning()) yield break;

            SpawnEnemies();
        }
    }

    private void CleanUpInactiveEnemies()
    {
        activeEnemies.RemoveWhere(e => e == null);
    }

    private bool ShouldStopSpawning()
    {
        if (activeEnemies.Count >= maxEnemiesInScene)
        {
            Log("Límite de enemigos alcanzado. Esperando espacio libre...");
            return true; // Detener el spawn temporalmente
        }

        if (maxEnemiesInTotal >= 0 && totalEnemiesSpawned >= maxEnemiesInTotal)
        {
            Log("Límite global de enemigos alcanzado.");
            return true; // Detener el spawn
        }

        return false; // Continuar spawn
    }

    private void SpawnEnemies()
    {
        int availableByTotal = (maxEnemiesInTotal >= 0) ? (maxEnemiesInTotal - totalEnemiesSpawned) : enemiesPerSpawn;
        int availableSpawnSlots = Mathf.Min(
        enemiesPerSpawn,
        maxEnemiesInScene - activeEnemies.Count,
        availableByTotal
        );

        if (availableSpawnSlots <= 0)
        {
            Log("No hay espacio para más enemigos.");
            return; // No hay espacio para más enemigos
        }

        for (int i = 0; i < availableSpawnSlots; i++)
        {
            var enemy = SpawnSingleEnemy();
            if (enemy != null)
            {
                totalEnemiesSpawned++;
                Log($"Enemigo {totalEnemiesSpawned} instanciado.");
            }
        }
    }

    public void SpawnWave(int totalEnemies)
    {
        int spawnerCount = Mathf.Min(spawnPoints.Length, 5);
        int perSpawner = totalEnemies / spawnerCount;
        int extra = totalEnemies % spawnerCount;
        int spawned = 0;

        for (int i = 0; i < spawnerCount; i++)
        {
            int toSpawn = perSpawner + (i < extra ? 1 : 0);
            for (int j = 0; j < toSpawn; j++)
            {
                var enemy = SpawnSingleEnemy();
                if (enemy != null)
                {
                    totalEnemiesSpawned++;
                    spawned++;
                    Log($"Enemigo {totalEnemiesSpawned} instanciado.");
                }
            }
        }

        if (spawned < totalEnemies)
        {
            Log($"Solo se pudieron instanciar {spawned} de {totalEnemies} enemigos solicitados.");
        }
    }

    private GameObject SpawnSingleEnemy()
    {
        // Filtrar solo los puntos habilitados
        var availablePoints = spawnPoints.Where(spawnPoint => spawnPoint.IsAvailable).ToList();

        if (availablePoints.Count == 0)
        {
            Log("No hay puntos de spawn disponibles para enemigos.");
            return null; // No hay puntos disponibles
        }

        GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        SpawnPoint randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = randomPoint.transform.position + 
                                new Vector3(offset.x, 0, offset.y);

        GameObject enemy = Instantiate(randomEnemy, spawnPosition, randomPoint.transform.rotation); // Instanciar enemigo

        activeEnemies.Add(enemy); // Agregado a la lista de enemigos activos
        return enemy;
    }

    public void ResetSpawner()
    {
        totalEnemiesSpawned = 0;
        activeEnemies.Clear();
        StopAllCoroutines(); // Por si acaso
        StartCoroutine(SpawnRoutine()); // Reinicia el spawn
    }

    // Visualizar el radio de spawn en el editor
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            Gizmos.DrawWireSphere(point.transform.position, spawnRadius);
            Gizmos.color = point.IsAvailable ? Color.green : Color.gray;
            Gizmos.DrawSphere(point.transform.position, 0.2f);
        }
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}