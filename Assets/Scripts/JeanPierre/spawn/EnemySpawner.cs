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
    [SerializeField] private int maxEnemiesTotal = 50; // Máximo de enemigos totales 
    [SerializeField] private SpawnPoint[] spawnPoints; // Lista de puntos con disponibilidad

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int totalEnemiesSpawned = 0; // Contador de enemigos totales creados

    void Start()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogError("¡Faltan prefabs de enemigos o puntos de spawn!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Limpia referencias nulas de enemigos muertos
            activeEnemies.RemoveAll(e => e == null);

            if (activeEnemies.Count >= maxEnemiesInScene)
            {
                Debug.Log("Límite de enemigos alcanzado. Esperando espacio libre...");
                continue; // Esperar hasta que haya espacio
            }

            if (totalEnemiesSpawned >= maxEnemiesTotal)
            {
                Debug.Log("Límite global de enemigos alcanzado.");
                yield break; // Se detiene el spawn
            }
            // Calcula cuántos enemigos se pueden crear
            int availableSpawnSlots = Mathf.Min(enemiesPerSpawn, maxEnemiesInScene - activeEnemies.Count, maxEnemiesTotal - totalEnemiesSpawned);

            for (int i = 0; i < availableSpawnSlots; i++)
            {
                var enemy = SpawnSingleEnemy();
                if (enemy != null)
                {
                    totalEnemiesSpawned++;
                    Debug.Log($"Enemigo {totalEnemiesSpawned} instanciado.");
                }
            }
        }
    }

    GameObject SpawnSingleEnemy()
    {
        // Filtrar solo los puntos habilitados
        var availablePoints = spawnPoints.Where(spawnPoint => spawnPoint.IsAvailable).ToList();

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn disponibles para enemigos.");
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
    }

    // Visualizar el radio de spawn en el editor
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            Gizmos.DrawWireSphere(point.transform.position, spawnRadius);
        }
    }
}