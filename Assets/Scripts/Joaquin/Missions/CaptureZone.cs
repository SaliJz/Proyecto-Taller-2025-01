using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CaptureZone : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject visualIndicator;

    private bool isActive = false;
    private bool playerInside = false;
    private bool enemyInside = false;

    public void Activate()
    {
        isActive = true;
        playerInside = false;
        enemyInside = false;
        if (visualIndicator != null) visualIndicator.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        playerInside = false;
        enemyInside = false;
        if (visualIndicator != null) visualIndicator.SetActive(false);

        MissionManager.Instance?.SetActiveCapture(false);
        MissionManager.Instance?.SetEnemyOnCaptureZone(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // Reporta el estado actual al MissionManager cada frame
        MissionManager.Instance?.SetActiveCapture(playerInside);
        MissionManager.Instance?.SetEnemyOnCaptureZone(enemyInside);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyInside = false;
        }
    }
}
