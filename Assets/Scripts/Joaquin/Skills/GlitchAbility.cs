using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlitchAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("GlitchTime Settings")]
    [SerializeField] private float cooldown = 12f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 50f;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float slowMultiplier = 0.5f;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    private bool canUse = true;
    private float currentCooldown = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse)
        {
            ActivateAbility();
        }

        if (!canUse)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0f)
            {
                canUse = true;
            }
        }
    }

    private void ActivateAbility()
    {
        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<GlitchShot>().Initialize(slowMultiplier, slowDuration);
        Destroy(projectile, projectileLifeTime);

        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("Glitch", currentCooldown, canUse);
    }
    /*
    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = targetPoint - projectileSpawnPoint.position;

        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }
    */
}
