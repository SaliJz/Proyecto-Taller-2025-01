using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElectroHackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("ElectroHack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
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

    [Header("UI")]
    [SerializeField] private TMP_Text cooldownText;

    private bool canUse = true;
    private float currentCooldown = 0f;

    private void Start()
    {
        cooldownText.text = "Glitch: Ready";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse)
        {
            ActivateAbility();
        }

        if (!canUse)
        {
            currentCooldown -= Time.deltaTime;
            cooldownText.text = $"Glitch: {Mathf.Ceil(currentCooldown)}";

            if (currentCooldown <= 0f)
            {
                canUse = true;
                cooldownText.text = "Glitch: Ready";
            }
        }
    }

    private void ActivateAbility()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = playerCamera.transform.forward * projectileSpeed;
        canUse = false;
        currentCooldown = cooldown;

        ElectroHackShot hack = projectile.AddComponent<ElectroHackShot>();
        hack.Initialize(radius, maxTargets, tickDamage, tickInterval, ticks, slowMultiplier);

        Destroy(projectile, projectileLifeTime);
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
