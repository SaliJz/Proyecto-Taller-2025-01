using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MindjackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float damagePerSecond = 20f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldown = 15f;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    private bool canUse = true;
    private float currentCooldown = 0;
    private float lastCooldownDisplay = -1f;

    private void Start()
    {
        HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse);
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

            if (Mathf.Ceil(currentCooldown) != Mathf.Ceil(lastCooldownDisplay))
            {
                HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse, cooldown);
                lastCooldownDisplay = currentCooldown;
            }

            if (currentCooldown <= 0f)
            {
                canUse = true;
                HUDManager.Instance.UpdateAbilityStatus("Mindjack", 0f, canUse, cooldown);
            }
        }
    }

    private void ActivateAbility()
    {
        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<MindjackShot>().Initialize(damagePerSecond, duration);
        Destroy(projectile, projectileLifeTime);

        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse);
    }
}