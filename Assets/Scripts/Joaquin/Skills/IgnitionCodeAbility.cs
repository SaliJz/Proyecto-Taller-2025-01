using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionCodeAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Ignition Settings")]
    [SerializeField] private float cooldown = 15f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float damagePerSecond = 8f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    private bool canUse = true;
    private float currentCooldown = 0;
    private float lastCooldownDisplay = -1f;

    private void Start()
    {
        HUDManager.Instance.UpdateAbilityStatus("IgniteCode", currentCooldown, canUse);
    }

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No se encontró la cámara principal. Asegúrate de que haya una cámara con la etiqueta 'MainCamera' en la escena.");
            }
        }
        if (projectileSpawnPoint == null)
        {
            Debug.LogError("Projectile Spawn Point no está asignado en MindjackAbility.");
        }
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab no está asignado en MindjackAbility.");
        }
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
            currentCooldown = Mathf.Max(0f, currentCooldown);

            // Solo actualiza si hay diferencia perceptible
            if (Mathf.Ceil(currentCooldown) != Mathf.Ceil(lastCooldownDisplay))
            {
                HUDManager.Instance.UpdateAbilityStatus("IgniteCode", currentCooldown, canUse, cooldown);
                lastCooldownDisplay = currentCooldown;
            }

            if (currentCooldown <= 0f)
            {
                canUse = true;
                HUDManager.Instance.UpdateAbilityStatus("IgniteCode", 0f, canUse, cooldown);
            }
        }
    }

    private void ActivateAbility()
    {
        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<IgnitionCodeShot>().Initialize(radius, damagePerSecond, duration, enemyLayer);
        Destroy(projectile, projectileLifeTime);

        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("IgniteCode", currentCooldown, canUse);
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
