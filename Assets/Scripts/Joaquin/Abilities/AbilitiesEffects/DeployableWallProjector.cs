using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableWallProjector : MonoBehaviour, IHackable
{
    [Header("Projector Settings")]
    [SerializeField] private GameObject energyWallPrefab;
    [SerializeField] private float activationCooldown = 10f;

    private bool isDeployed = false;

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (isDeployed || energyWallPrefab == null)
        {
            return;
        }

        Debug.Log("Proyector activado. Desplegando muro de energía.");

        GameObject wallInstance = Instantiate(energyWallPrefab, transform.position, transform.rotation);

        EnergyWall energyWall = wallInstance.GetComponent<EnergyWall>();
        if (energyWall != null)
        {
            energyWall.Initialize(duration);
        }

        StartCoroutine(DeploymentRoutine(duration));
    }

    private IEnumerator DeploymentRoutine(float activeDuration)
    {
        isDeployed = true;

        yield return new WaitForSeconds(activeDuration + activationCooldown);

        isDeployed = false;
        Debug.Log("Proyector listo para ser activado de nuevo.");
    }
}