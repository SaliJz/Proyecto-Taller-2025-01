using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GeneradorUnicoDeEnemigosAleatoriosYUbicaciones : MonoBehaviour
{
    [Header("Configuración de Enemigos")]
    [Tooltip("Array con los 4 prefabs de enemigos")]
    public GameObject[] enemyPrefabs = new GameObject[4];

    [Header("Puntos de Spawn")]
    [Tooltip("Array con las posiciones donde spawnear")]
    public Transform[] spawnPoints;

    void Start()
    {
        GenerarEnemigosUnicos();
    }

    void GenerarEnemigosUnicos()
    {
        if (enemyPrefabs.Length < 2 || spawnPoints.Length < 2)
        {
            Debug.LogWarning("Se requieren al menos 4 prefabs y 2 puntos de spawn.");
            return;
        }

        List<int> usadosPrefabs = new List<int>();
        List<int> usadosSpawns = new List<int>();

        // Genera exactamente 2 enemigos distintos en posiciones distintas
        for (int i = 0; i < 2; i++)
        {
            int idxPrefab;
            do { idxPrefab = Random.Range(0, enemyPrefabs.Length); }
            while (usadosPrefabs.Contains(idxPrefab));
            usadosPrefabs.Add(idxPrefab);

            int idxSpawn;
            do { idxSpawn = Random.Range(0, spawnPoints.Length); }
            while (usadosSpawns.Contains(idxSpawn));
            usadosSpawns.Add(idxSpawn);

            Vector3 pos = spawnPoints[idxSpawn].position;
            // Instantiate sin parent: quedan en la raíz de la jerarquía
            Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);
        }
    }
}




//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[DisallowMultipleComponent]
//public class GeneradorUnicoDeEnemigosAleatoriosYUbicaciones : MonoBehaviour
//{
//    [Header("Configuración de Enemigos")]
//    public GameObject[] enemyPrefabs = new GameObject[4];

//    [Header("Puntos de Spawn")]
//    public Transform[] spawnPoints;

//    [Header("Opciones de Spawn")]
//    [Range(1, 4)]
//    public int enemiesToSpawn = 2;

//    [Header("Intervalo entre spawns (segundos)")]
//    public float spawnInterval = 1f;

//    private void Start()
//    {
//        StartCoroutine(SpawnDistinctEnemiesWithInterval());
//    }

//    private IEnumerator SpawnDistinctEnemiesWithInterval()
//    {
//        if (enemyPrefabs.Length < enemiesToSpawn || spawnPoints.Length < enemiesToSpawn)
//        {
//            Debug.LogError("No hay suficientes prefabs o puntos de spawn para instanciar.");
//            yield break;
//        }

//        List<int> enemyIndices = new List<int>();
//        for (int i = 0; i < enemyPrefabs.Length; i++)
//            enemyIndices.Add(i);

//        List<int> spawnIndices = new List<int>();
//        for (int i = 0; i < spawnPoints.Length; i++)
//            spawnIndices.Add(i);

//        for (int i = 0; i < enemiesToSpawn; i++)
//        {
//            int randomEnemyListIndex = Random.Range(0, enemyIndices.Count);
//            int enemyIndex = enemyIndices[randomEnemyListIndex];
//            enemyIndices.RemoveAt(randomEnemyListIndex);

//            int randomSpawnListIndex = Random.Range(0, spawnIndices.Count);
//            int spawnIndex = spawnIndices[randomSpawnListIndex];
//            spawnIndices.RemoveAt(randomSpawnListIndex);

//            Instantiate(
//                enemyPrefabs[enemyIndex],
//                spawnPoints[spawnIndex].position,
//                Quaternion.identity,
//                transform
//            );

//            // Espera antes de generar el siguiente enemigo
//            yield return new WaitForSeconds(spawnInterval);
//        }
//    }
//}
