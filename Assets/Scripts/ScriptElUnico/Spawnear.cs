using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnear : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnArea;
    public Transform player; 
    public int totalEnemies = 20;
    public float spawnDelay = 1.5f;

    private int enemiesSpawned = 0;


    void Start()
    {
        StartCoroutine(SpawnEnemiesOverTime());
    }

    IEnumerator SpawnEnemiesOverTime()
    {
        while (enemiesSpawned < totalEnemies)
        {
            Vector3 spawnPos = spawnArea.position + new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10));
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Enemigo instanciado en: " + spawnPos);


            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            if (movement != null)
                movement.target = player;


            enemiesSpawned++;
            yield return new WaitForSeconds(spawnDelay);
        }
    }

}
