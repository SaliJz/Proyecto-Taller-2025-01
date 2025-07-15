using System.Collections;
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

        do
        {
            newIndex = Random.Range(0, spawnPoints.Length);
        } while (newIndex == lastSpawnIndex && spawnPoints.Length > 1);

        PlayerPrefs.SetInt("LastSpawnIndex", newIndex);
        PlayerPrefs.Save();

        currentSpawnPoint = spawnPoints[newIndex];

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player != null)
        {
            player.position = currentSpawnPoint.position;
        }

        currentSpawnPoint.GetComponent<SpawnPoint>().SetTemporarilyInactive();

        if (player != null)
        {
            StartCoroutine(WaitForPlayerThenCheckDistance());
        }
        else
        {
            Debug.LogWarning("Player no encontrado al iniciar PlayerSpawnManager.");
        }
    }

    private IEnumerator CheckPlayerDistance(Transform player)
    {
        if (player == null || currentSpawnPoint == null)
        {
            Debug.LogWarning("CheckPlayerDistance: Referencia nula detectada. Coroutine abortada.");
            yield break;
        }

        while (true)
        {
            if (Vector3.Distance(player.position, currentSpawnPoint.position) > reenableDistance)
            {
                currentSpawnPoint.GetComponent<SpawnPoint>().SetActiveForEnemies();
                yield break;
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private IEnumerator WaitForPlayerThenCheckDistance()
    {
        Transform player = null;
        while (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                break;
            }
            yield return null; // Espera un frame
        }

        // Ya tienes el player
        StartCoroutine(CheckPlayerDistance(player));
    }

    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("LastSpawnIndex");
        PlayerPrefs.Save();
    }
}
