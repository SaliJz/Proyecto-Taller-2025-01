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

    [Header("Control de Stop (deprecated)")]
    [Tooltip("Sólo para debugging en Inspector; use StopAndDestroyAllEnemies()")]
    public bool stopSpawning = false;

    // Lista para seguir las instancias activas
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool hasStopped = false;
    private Coroutine spawnRoutine;

    void Start()
    {
        spawnRoutine = StartCoroutine(GenerarEnemigosPeriodicamente());
        GenerarEnemigosUnicos();
    }

    void Update()
    {
        // Sólo para debug manual en Inspector
        if (stopSpawning && !hasStopped)
        {
            StopAndDestroyAllEnemies();
        }
    }

    IEnumerator GenerarEnemigosPeriodicamente()
    {
        while (!hasStopped)
        {
            yield return new WaitForSeconds(spawnInterval);
            GenerarEnemigosUnicos();
        }
    }

    void GenerarEnemigosUnicos()
    {
        if (hasStopped) return;

        activeEnemies.RemoveAll(e => e == null);
        if (activeEnemies.Count >= maxActiveEnemies) return;
        if (spawnCount < 1 || spawnCount > enemyPrefabs.Length || spawnCount > spawnPoints.Length) return;

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
            activeEnemies.Add(nuevo);
        }

        Debug.Log($"Generados {cantidadAProceder} enemigos. Activos ahora: {activeEnemies.Count}/{maxActiveEnemies}.");
    }

    /// <summary>
    /// Detiene inmediatamente el spawn y destruye todos los enemigos activos.
    /// </summary>
    public void StopAndDestroyAllEnemies()
    {
        if (hasStopped) return;
        hasStopped = true;
        StopAllCoroutines();
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
        Debug.Log("Spawn detenido y todos los enemigos destruidos (método directo).");
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
//    [Header("Cantidad de Enemigos por Oleada")]
//    public int spawnCount = 2;
//    [Header("Intervalo de Spawn")]
//    public float spawnInterval = 5f;
//    [Header("Máximo de Enemigos Simultáneos")]
//    public int maxActiveEnemies = 3;
//    [Header("Control de Stop")]
//    public bool stopSpawning = false;

//    private List<GameObject> activeEnemies = new List<GameObject>();
//    private bool hasStopped = false;
//    private Coroutine spawnRoutine;  // ← nueva referencia

//    void Start()
//    {
//        // guardamos la coroutine en spawnRoutine
//        spawnRoutine = StartCoroutine(GenerarEnemigosPeriodicamente());
//        GenerarEnemigosUnicos();
//    }

//    void Update()
//    {
//        if (stopSpawning && !hasStopped)
//        {
//            DetenerYDestruirTodo();
//        }
//    }

//    IEnumerator GenerarEnemigosPeriodicamente()
//    {
//        while (!hasStopped)
//        {
//            yield return new WaitForSeconds(spawnInterval);
//            GenerarEnemigosUnicos();
//        }
//    }

//    void GenerarEnemigosUnicos()
//    {
//        if (hasStopped) return;
//        activeEnemies.RemoveAll(e => e == null);
//        if (activeEnemies.Count >= maxActiveEnemies) return;
//        if (spawnCount < 1 || spawnCount > enemyPrefabs.Length || spawnCount > spawnPoints.Length) return;

//        int slotsDisponibles = maxActiveEnemies - activeEnemies.Count;
//        int cantidadAProceder = Mathf.Min(spawnCount, slotsDisponibles);

//        var usadosPrefabs = new List<int>();
//        var usadosSpawns = new List<int>();

//        for (int i = 0; i < cantidadAProceder; i++)
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
//            GameObject nuevo = Instantiate(enemyPrefabs[idxPrefab], pos, Quaternion.identity);
//            activeEnemies.Add(nuevo);
//        }

//        Debug.Log($"Generados {cantidadAProceder} enemigos. Activos ahora: {activeEnemies.Count}/{maxActiveEnemies}.");
//    }

//    void DetenerYDestruirTodo()
//    {
//        hasStopped = true;                       // ← movido aquí
//        StopAllCoroutines();                     // ← detiene TODO, incluyendo GenerarEnemigosPeriodicamente
//        foreach (var enemy in activeEnemies)     // ← destruye instancias vivas
//            if (enemy != null)
//                Destroy(enemy);
//        activeEnemies.Clear();
//        Debug.Log("Se ha detenido el spawn y destruido todos los enemigos.");
//    }
//}










