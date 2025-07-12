// GeneradorUnicoDeEnemigosAleatoriosYUbicaciones.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GeneradorUnicoDeEnemigosAleatoriosYUbicaciones : MonoBehaviour
{
    [Header("Configuración de Enemigos")]
    [Tooltip("Array con los 4 prefabs de enemigos")]
    public GameObject[] enemyPrefabs = new GameObject[4];

    [Header("Puntos de Spawn")]
    [Tooltip("Array con las posiciones (Transforms) donde pueden aparecer los enemigos")]
    public Transform[] spawnPoints;

    [Header("Opciones de Spawn")]
    [Tooltip("Cuántos enemigos distintos quieres instanciar")]
    [Range(1, 4)]
    public int enemiesToSpawn = 2;

    private void Start()
    {
        SpawnDistinctEnemies();
    }

    private void SpawnDistinctEnemies()
    {
        // Validaciones
        if (enemyPrefabs.Length < enemiesToSpawn)
        {
            Debug.LogError($"No hay suficientes prefabs de enemigos en el array para instanciar {enemiesToSpawn} distintos.");
            return;
        }
        if (spawnPoints.Length < enemiesToSpawn)
        {
            Debug.LogError($"No hay suficientes puntos de spawn en el array para colocar {enemiesToSpawn} enemigos sin solaparse.");
            return;
        }

        // Listas de índices para selección sin repetición
        List<int> enemyIndices = new List<int>();
        for (int i = 0; i < enemyPrefabs.Length; i++)
            enemyIndices.Add(i);

        List<int> spawnIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            spawnIndices.Add(i);

        // Selección e instanciación
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Elegir índice aleatorio de enemigo
            int randomEnemyListIndex = Random.Range(0, enemyIndices.Count);
            int enemyIndex = enemyIndices[randomEnemyListIndex];
            enemyIndices.RemoveAt(randomEnemyListIndex);

            // Elegir índice aleatorio de punto de spawn
            int randomSpawnListIndex = Random.Range(0, spawnIndices.Count);
            int spawnIndex = spawnIndices[randomSpawnListIndex];
            spawnIndices.RemoveAt(randomSpawnListIndex);

            // Instanciar el enemigo en la posición deseada
            Instantiate(
                enemyPrefabs[enemyIndex],
                spawnPoints[spawnIndex].position,
                Quaternion.identity,
                this.transform // opcional: padre para organización en jerarquía
            );
        }
    }
}
