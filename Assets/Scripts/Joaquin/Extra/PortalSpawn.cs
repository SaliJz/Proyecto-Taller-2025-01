using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpawn : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float spawnDelay = 1f;

    private void Start()
    {
        if (portalPrefab == null)
        {
            Debug.LogError("Portal prefab is not assigned in the inspector.");
            return;
        }

        StartCoroutine(PortalStateManager());
    }

    private IEnumerator PortalStateManager()
    {
        if (portalPrefab == null) yield break;

        portalPrefab.SetActive(true);
        yield return new WaitForSeconds(spawnDelay);
        portalPrefab.SetActive(false);
    }
}