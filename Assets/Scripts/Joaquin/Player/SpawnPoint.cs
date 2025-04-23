using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float gizmosRange = 1f; // Radio de la esfera para Gizmos
    private bool isAvailable = true;

    public bool IsAvailable => isAvailable;

    public void SetTemporarilyInactive()
    {
        isAvailable = false;
    }

    public void SetActiveForEnemies()
    {
        isAvailable = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, gizmosRange);
    }
}
