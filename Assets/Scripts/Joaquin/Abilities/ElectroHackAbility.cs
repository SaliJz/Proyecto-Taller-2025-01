using UnityEngine;
using System.Collections;

public class ElectroHackAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private GameObject electroHackVFX;

    [Header("ElectroHack Settings")]
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 20f;

    [SerializeField] private float baseCooldown = 10f;
    [SerializeField] private float baseEnemiesAffected = 3;
    [SerializeField] private float slowMultiplier = 0.75f;
    [SerializeField] private float baseDamagePerSecond = 8f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Animation")]
    [SerializeField] private int abilityAnimationID = 1;

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

        if (projectileSpawnPoint == null) Debug.LogError("Projectile Spawn Point no está asignado en MindjackAbility.");

        if (projectilePrefab == null) Debug.LogError("Projectile Prefab no está asignado en MindjackAbility.");

        if (abilityInfo == null)
        {
            abilityInfo = GetComponent<AbilityInfo>();
            if (abilityInfo == null) Debug.LogError("AbilityInfo no está asignado en ElectroHackAbility.");
        }
    }

    private IEnumerator Start()
    {
        ApplyUpgrades();
        yield return new WaitForEndOfFrame();

        HUDManager.Instance?.UpdateAbilityUI(gameObject);
        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void OnEnable()
    {
        if (electroHackVFX != null)
        {
            electroHackVFX.SetActive(true);
        }

        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);
    }

    private void OnDisable()
    {
        if (electroHackVFX != null)
        {
            electroHackVFX.SetActive(false);
        }
    }

    private void ApplyUpgrades()
    {
        AbilityStatsData abilityStatsData = DataManager.GetAbilityStats(abilityInfo.abilityName);
        if (abilityStatsData == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f; // Reduce cooldown by 1 second per level
        const float DURATION_INCREASE_PER_LEVEL = 1.0f; // Increase duration by 1 second per level
        const float ENEMIES_AFFECTED_INCREASE_PER_LEVEL = 1.0f; // Increase enemies affected by 1 per level
        const float DAMAGE_INCREASE_PER_LEVEL = 2.0f; // Increase damage per second by 2 per level

        currentCooldown = baseCooldown - (abilityStatsData.Level * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDuration = baseDuration + (abilityStatsData.Level * DURATION_INCREASE_PER_LEVEL);
        currentDamagePerSecond = baseDamagePerSecond + (abilityStatsData.Level * DAMAGE_INCREASE_PER_LEVEL);
        currentEnemiesAffected = baseEnemiesAffected + (abilityStatsData.Level * ENEMIES_AFFECTED_INCREASE_PER_LEVEL);

        Debug.Log($"ElectroHack Ability Upgraded: Cooldown={currentCooldown}, Duration={currentDuration}, DamagePerSecond={currentDamagePerSecond}, EnemiesAffected={currentEnemiesAffected}");
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
        PlayEffect();
        canUse = false;
        currentCooldownTimer = currentCooldown;
        HUDManager.Instance.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);

        PlayerAnimatorController.Instance?.PlayFireAbilityAnim(abilityAnimationID);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<ElectroHackShot>().Initialize(baseRadius, currentEnemiesAffected, currentDamagePerSecond, currentDuration, slowMultiplier, targetLayer);
        Destroy(projectile, projectileLifeTime);
    }

    #region Effects

    private void PlayEffect()
    {
        if (muzzleEffect != null) muzzleEffect.Play();
    }

    #endregion
}