using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MindjackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float baseCooldown = 15f;
    [SerializeField] private float baseDamagePerSecond = 20f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseRadius = 5f;

    private float currentCooldown;
    private float currentDamagePerSecond;

    private bool canUse = true;
    private float currentCooldownTimer = 0;
    private float lastCooldownDisplay = -1f;
    private AbilityInfo abilityInfo;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No se encontr� la c�mara principal. Aseg�rate de que haya una c�mara con la etiqueta 'MainCamera' en la escena.");
            }
        }

        if (projectileSpawnPoint == null) Debug.LogError("Projectile Spawn Point no est� asignado en MindjackAbility.");

        if (projectilePrefab == null) Debug.LogError("Projectile Prefab no est� asignado en MindjackAbility.");

        if (abilityInfo == null)
        {
            abilityInfo = GetComponent<AbilityInfo>();
            if (abilityInfo == null) Debug.LogError("AbilityInfo no est� asignado en MindjackAbility.");
        }
    }

    private void Start()
    {
        ApplyUpgrades();
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void ApplyUpgrades()
    {
        AbilityStats stats = AbilityShopDataManager.GetStats(abilityInfo.abilityName);
        if (stats == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f;
        const float DAMAGE_INCREASE_PER_LEVEL = 2.0f;

        currentCooldown = baseCooldown - (stats.CooldownLevel * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDamagePerSecond = baseDamagePerSecond + (stats.DamageLevel * DAMAGE_INCREASE_PER_LEVEL);

        Debug.Log($"Mindjack Ability Upgrades Applied: Cooldown={currentCooldown}, DamagePerSecond={currentDamagePerSecond}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse) ActivateAbility();
        if (!canUse) CooldownLogic();
    }

    private void CooldownLogic()
    {
        currentCooldownTimer -= Time.deltaTime;
        if (Mathf.Ceil(currentCooldownTimer) != Mathf.Ceil(lastCooldownDisplay))
        {
            HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, false, currentCooldown);
            lastCooldownDisplay = currentCooldownTimer;
        }
        if (currentCooldownTimer <= 0)
        {
            canUse = true;
            HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, 0, true, currentCooldown);
        }
    }

    private void ActivateAbility()
    {
        canUse = false;
        currentCooldownTimer = currentCooldown;
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().linearVelocity = direction * projectileSpeed;

        projectile.GetComponent<MindjackShot>().Initialize(baseRadius, currentDamagePerSecond, baseDuration);
        Destroy(projectile, projectileLifeTime);
    }
}