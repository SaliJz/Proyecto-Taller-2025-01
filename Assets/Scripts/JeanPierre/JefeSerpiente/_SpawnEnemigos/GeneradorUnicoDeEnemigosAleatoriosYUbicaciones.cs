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

    [Header("Cantidad de Enemigos por Oleada")]
    [Tooltip("Cuántos enemigos generar cada oleada (p. ej. 2 o 3)")]
    public int spawnCount = 2;

    [Header("Intervalo de Spawn")]
    [Tooltip("Tiempo en segundos entre cada generación")]
    public float spawnInterval = 5f;

    [Header("Máximo de Enemigos Simultáneos")]
    [Tooltip("Número máximo de enemigos activos antes de frenar el spawn")]
    public int maxActiveEnemies = 3;

    // Lista para seguir las instancias activas
    private List<GameObject> activeEnemies = new List<GameObject>();

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
        // Limpiar referencias a enemigos destruidos
        activeEnemies.RemoveAll(e => e == null);

        // Si ya hay maxActiveEnemies o más, no generar nada
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            Debug.Log($"Hay {activeEnemies.Count} enemigos activos. Esperando a que queden menos de {maxActiveEnemies} para generar más.");
            return;
        }

        // Verificaciones básicas
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

        // Calculamos cuántos podemos spawnear realmente
        int slotsDisponibles = maxActiveEnemies - activeEnemies.Count;
        int cantidadAProceder = Mathf.Min(spawnCount, slotsDisponibles);

        var usadosPrefabs = new List<int>();
        var usadosSpawns = new List<int>();

        for (int i = 0; i < cantidadAProceder; i++)
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
            GameObject nuevo = Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);

            // Registramos la instancia en la lista
            activeEnemies.Add(nuevo);
        }

        Debug.Log($"Generados {cantidadAProceder} enemigos. Activos ahora: {activeEnemies.Count}/{maxActiveEnemies}.");
    }
}









//using System.Collections;
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

//    [Header("Cantidad de Enemigos")]
//    [Tooltip("Cuántos enemigos generar cada oleada (p. ej. 2 o 3)")]
//    public int spawnCount = 2;

//    [Header("Intervalo de Spawn")]
//    [Tooltip("Tiempo en segundos entre cada generación")]
//    public float spawnInterval = 5f;

//    void Start()
//    {
//        GenerarEnemigosUnicos();
//        StartCoroutine(GenerarEnemigosPeriodicamente());
//    }

//    IEnumerator GenerarEnemigosPeriodicamente()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(spawnInterval);
//            GenerarEnemigosUnicos();
//        }
//    }

//    void GenerarEnemigosUnicos()
//    {
//        // Verificaciones
//        if (spawnCount < 1)
//        {
//            Debug.LogWarning("spawnCount debe ser al menos 1.");
//            return;
//        }
//        if (spawnCount > enemyPrefabs.Length || spawnCount > spawnPoints.Length)
//        {
//            Debug.LogWarning($"spawnCount no puede exceder prefabs ({enemyPrefabs.Length}) ni puntos ({spawnPoints.Length}).");
//            return;
//        }

//        var usadosPrefabs = new List<int>();
//        var usadosSpawns = new List<int>();

//        for (int i = 0; i < spawnCount; i++)
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
//            Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);
//        }
//    }
//}




