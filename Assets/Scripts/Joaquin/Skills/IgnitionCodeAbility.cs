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
    [SerializeField] private float baseCooldown = 15f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float damagePerSecond = 8f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float baseRadius = 3f;

    [SerializeField] private bool isNivel1 = false; 

    private float currentCooldown;
    private float currentDuration;
    private float currentRadius;

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

        if (abilityInfo == null)
        {
            abilityInfo = GetComponent<AbilityInfo>();
            if (abilityInfo == null)
            {
                Debug.LogError("AbilityInfo no está asignado en IgniteCodeAbility.");
            }
        }

        if (isNivel1) AbilityUpgradeManager.ResetUpgrades();
        ApplyUpgrades();
    }

    private void Start()
    {
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void OnEnable()
    {
        AbilityUpgradeManager.OnUpgradesChanged += ApplyUpgrades;
    }

    private void OnDisable()
    {
        AbilityUpgradeManager.OnUpgradesChanged -= ApplyUpgrades;
    }

    private void ApplyUpgrades()
    {
        // Cooldown
        float cooldownReduction = AbilityUpgradeManager.CooldownLevel * AbilityUpgradeManager.COOLDOWN_REDUCTION;
        currentCooldown = baseCooldown - (cooldownReduction);

        // Duración
        float durationBonus = AbilityUpgradeManager.EffectDurationLevel * AbilityUpgradeManager.DURATION_INCREASE_PERCENT;
        currentDuration = baseDuration * (1 + durationBonus);

        // Rango
        float rangeBonus = AbilityUpgradeManager.EffectRangeLevel * AbilityUpgradeManager.RANGE_INCREASE_PERCENT;
        currentRadius = baseRadius * (1 + rangeBonus);

        Debug.Log($"Stats de {abilityInfo.abilityName} actualizados!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse)
        {
            ActivateAbility();
        }

        if (!canUse)
        {
            currentCooldownTimer -= Time.deltaTime;
            currentCooldownTimer = Mathf.Max(0f, currentCooldownTimer);

            if (Mathf.Ceil(currentCooldownTimer) != Mathf.Ceil(lastCooldownDisplay))
            {
                HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);
                lastCooldownDisplay = currentCooldownTimer;
            }

            if (currentCooldownTimer <= 0f)
            {
                canUse = true;
                HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, 0f, canUse, currentCooldown);
            }
        }
    }

    private void ActivateAbility()
    {
        canUse = false;
        currentCooldownTimer = baseCooldown;
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<IgnitionCodeShot>().Initialize(currentRadius, damagePerSecond, currentDuration, enemyLayer);
        Destroy(projectile, projectileLifeTime);
    }
}