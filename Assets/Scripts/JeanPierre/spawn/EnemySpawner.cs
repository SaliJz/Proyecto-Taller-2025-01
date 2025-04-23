using UnityEngine;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public GameObject[] enemyPrefabs;       // Prefabs de enemigos
    public float spawnInterval = 3f;        // Tiempo entre oleadas
    public int enemiesPerSpawn = 5;         // Enemigos a crear por intervalo
    public float spawnRadius = 2f;          // Radio de dispersión alrededor del punto

    [SerializeField] private int maxEnemiesInScene = 20; // Máximo de enemigos en la escena
    [SerializeField] private SpawnPoint[] spawnPoints; // Lista de puntos con disponibilidad

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

            int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (currentEnemies >= maxEnemiesInScene)
            {
                Debug.Log("Límite de enemigos alcanzado. Esperando espacio libre...");
                continue; // Esperar hasta que haya espacio
            }

            int spawnsAllowed = Mathf.Min(enemiesPerSpawn, maxEnemiesInScene - currentEnemies);

            for (int i = 0; i < spawnsAllowed; i++)
            {
                SpawnSingleEnemy();
                Debug.Log($"Enemigo {i + 1} de {spawnsAllowed} creado.");
            }
        }
    }

    void SpawnSingleEnemy()
    {
        // Filtrar solo los puntos habilitados
        var availablePoints = spawnPoints.Where(p => p.IsAvailable).ToList();

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn disponibles para enemigos.");
            return;
        }

        GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        SpawnPoint randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = randomPoint.transform.position + 
                                new Vector3(offset.x, 0, offset.y);

        Instantiate(randomEnemy, spawnPosition, randomPoint.transform.rotation);
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