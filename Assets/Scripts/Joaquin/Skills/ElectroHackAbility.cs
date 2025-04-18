using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElectroHackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("ElectroHack Settings")]
    [SerializeField] private float cooldown = 10f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private int maxTargets = 3;
    [SerializeField] private float tickDamage = 15;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private int ticks = 2;
    [SerializeField] private float slowMultiplier = 0.75f; // 25% menos velocidad

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

        projectile.GetComponent<ElectroHackShot>().Initialize(radius, maxTargets, tickDamage, tickInterval, ticks, slowMultiplier);
        Destroy(projectile, projectileLifeTime);

        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("Hack", currentCooldown, canUse);
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
