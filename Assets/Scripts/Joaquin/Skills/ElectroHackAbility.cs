using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElectroHackAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private ParticleSystem muzzleEffect;

    [Header("ElectroHack Settings")]
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float baseCooldown = 10f;
    [SerializeField] private float baseEnemiesAffected = 3;
    [SerializeField] private float tickDamage = 15;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float ticks = 2;
    [SerializeField] private float slowMultiplier = 0.75f;
    [SerializeField] private float baseRadius = 5f;

    private float currentCooldown;
    private float currentDuration;
    private float currentEnemiesAffected;
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
                Debug.LogError("AbilityInfo no está asignado en ElectroHackAbility.");
            }
        }

        ApplyUpgrades();
    }

    private void Start()
    {
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void OnEnable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged += ApplyUpgrades;
    }

    private void OnDisable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged -= ApplyUpgrades;
    }

    private void ApplyUpgrades()
    {
        AbilityStats stats = AbilityShopDataManager.GetStats(AbilityType.ElectroHack);
        if (stats == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f;
        const float DURATION_INCREASE_PER_LEVEL = 0.5f;
        const float DAMAGE_INCREASE_PER_LEVEL = 2.0f;
        const float ENEMIES_AFFECTED_INCREASE_PER_LEVEL = 1.0f;

        currentCooldown = baseCooldown - (stats.CooldownLevel * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDuration = ticks + (stats.DurationLevel * DURATION_INCREASE_PER_LEVEL);
        currentDamagePerSecond = tickDamage + (stats.DamageLevel * DAMAGE_INCREASE_PER_LEVEL);
        currentEnemiesAffected = baseEnemiesAffected + (stats.EnemiesAffectedLevel * ENEMIES_AFFECTED_INCREASE_PER_LEVEL);

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
        PlayEffect();
        canUse = false;
        currentCooldownTimer = currentCooldown;
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<ElectroHackShot>().Initialize(baseRadius, currentEnemiesAffected, currentDamagePerSecond, tickInterval, currentDuration, slowMultiplier);
        Destroy(projectile, projectileLifeTime);
    }

    #region Effects

    private void PlayEffect()
    {
        if (muzzleEffect != null) muzzleEffect.Play();
    }

    #endregion
}
