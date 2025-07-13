using System.Collections;
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

    [Header("Cantidad de Enemigos")]
    [Tooltip("Cuántos enemigos generar cada oleada (p. ej. 2 o 3)")]
    public int spawnCount = 2;

    [Header("Intervalo de Spawn")]
    [Tooltip("Tiempo en segundos entre cada generación")]
    public float spawnInterval = 5f;

    void Start()
    {
        GenerarEnemigosUnicos();
        StartCoroutine(GenerarEnemigosPeriodicamente());
    }

    IEnumerator GenerarEnemigosPeriodicamente()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            GenerarEnemigosUnicos();
        }
    }

    void GenerarEnemigosUnicos()
    {
        // Verificaciones
        if (spawnCount < 1)
        {
            Debug.LogWarning("spawnCount debe ser al menos 1.");
            return;
        }
        if (spawnCount > enemyPrefabs.Length || spawnCount > spawnPoints.Length)
        {
            Debug.LogWarning($"spawnCount no puede exceder prefabs ({enemyPrefabs.Length}) ni puntos ({spawnPoints.Length}).");
            return;
        }

        var usadosPrefabs = new List<int>();
        var usadosSpawns = new List<int>();

        for (int i = 0; i < spawnCount; i++)
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
            Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);
        }
    }
}


//using System.Collections.Generic;
//using UnityEngine;

//[DisallowMultipleComponent]
//public class GeneradorUnicoDeEnemigosAleatoriosYUbicaciones : MonoBehaviour
//{
//    [Header("Configuración de Enemigos")]
//    [Tooltip("Array con los 4 prefabs de enemigos")]
//    public GameObject[] enemyPrefabs = new GameObject[4];

//    [Header("Puntos de Spawn")]
//    [Tooltip("Array con las posiciones donde spawnear")]
//    public Transform[] spawnPoints;

//    void Start()
//    {
//        GenerarEnemigosUnicos();
//    }

//    void GenerarEnemigosUnicos()
//    {
//        if (enemyPrefabs.Length < 2 || spawnPoints.Length < 2)
//        {
//            Debug.LogWarning("Se requieren al menos 4 prefabs y 2 puntos de spawn.");
//            return;
//        }

//        List<int> usadosPrefabs = new List<int>();
//        List<int> usadosSpawns = new List<int>();

//        // Genera exactamente 2 enemigos distintos en posiciones distintas
//        for (int i = 0; i < 2; i++)
//        {
//            int idxPrefab;
//            do { idxPrefab = Random.Range(0, enemyPrefabs.Length); }
//            while (usadosPrefabs.Contains(idxPrefab));
//            usadosPrefabs.Add(idxPrefab);

//            int idxSpawn;
//            do { idxSpawn = Random.Range(0, spawnPoints.Length); }
//            while (usadosSpawns.Contains(idxSpawn));
//            usadosSpawns.Add(idxSpawn);

//            Vector3 pos = spawnPoints[idxSpawn].position;
//            // Instantiate sin parent: quedan en la raíz de la jerarquía
//            Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);
//        }
//    }
//}




