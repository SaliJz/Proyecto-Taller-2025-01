using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float reenableDistance = 10f;

    private Transform currentSpawnPoint;
    private static int lastSpawnIndex = -1;

    private void Start()
    {
        int newIndex;
        int lastIndex = PlayerPrefs.GetInt("LastSpawnIndex", -1);

        // Si el índice de spawn guardado es válido y diferente al último spawn
        do
        {
            newIndex = Random.Range(0, spawnPoints.Length);
        } while (newIndex == lastSpawnIndex && spawnPoints.Length > 1);

        PlayerPrefs.SetInt("LastSpawnIndex", newIndex); // Se guarda
        PlayerPrefs.Save();

        currentSpawnPoint = spawnPoints[newIndex];

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            player.position = currentSpawnPoint.position;
        }

        currentSpawnPoint.GetComponent<SpawnPoint>().SetTemporarilyInactive();

        StartCoroutine(CheckPlayerDistance(player));
    }

    private IEnumerator CheckPlayerDistance(Transform player)
    {
        while (true)
        {
            if (Vector3.Distance(player.position, currentSpawnPoint.position) > reenableDistance)
            {
                currentSpawnPoint.GetComponent<SpawnPoint>().SetActiveForEnemies();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("LastSpawnIndex");
        PlayerPrefs.Save();
    }
}
