using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public GameObject[] enemyPrefabs;       // Prefabs de enemigos
    public Transform[] spawnPoints;         // Puntos de spawn
    public float spawnInterval = 3f;        // Tiempo entre oleadas
    public int enemiesPerSpawn = 5;         // Enemigos a crear por intervalo
    public float spawnRadius = 2f;          // Radio de dispersión alrededor del punto

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

            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                SpawnSingleEnemy();
            }
        }
    }

    void SpawnSingleEnemy()
    {
        GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Calcular posición aleatoria alrededor del punto de spawn
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = randomSpawnPoint.position +
                                new Vector3(randomOffset.x, 0f, randomOffset.y);

        Instantiate(randomEnemy, spawnPosition, randomSpawnPoint.rotation);
    }

    // Visualizar el radio de spawn en el editor
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
        }
    }
}