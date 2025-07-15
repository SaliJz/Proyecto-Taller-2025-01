using UnityEngine;
using System.Collections;

public class IgnitionCodeAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 40f;

    [Header("Ignition Code Settings")]
    [SerializeField] private float baseCooldown = 15f;
    [SerializeField] private float baseDamagePerSecond = 8f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private int abilityAnimationID = 1;

    private float currentCooldown;
    private float currentDuration;
    private float currentRadius;
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

        if (projectileSpawnPoint == null) Debug.LogError("Projectile Spawn Point no está asignado en IgnitionCodeAbility.");

        if (projectilePrefab == null) Debug.LogError("Projectile Prefab no está asignado en IgnitionCodeAbility.");

        if (abilityInfo == null)
        {
            abilityInfo = GetComponent<AbilityInfo>();
            if (abilityInfo == null) Debug.LogError("AbilityInfo no está asignado en IgniteCodeAbility.");
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
        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);
    }

    private void ApplyUpgrades()
    {
        AbilityStatsData abilityStatsData = DataManager.GetAbilityStats(abilityInfo.abilityName);
        if (abilityStatsData == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f; // Reduce cooldown by 1 second per level
        const float DURATION_INCREASE_PER_LEVEL = 1.0f; // Increase duration by 1 second per level
        const float RANGE_INCREASE_PER_LEVEL = 0.25f; // Increase radius by 0.25 units per level
        const float DAMAGE_INCREASE_PER_LEVEL = 2.0f; // Increase damage per second by 2 per level

        currentCooldown = baseCooldown - (abilityStatsData.Level * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDuration = baseDuration + (abilityStatsData.Level * DURATION_INCREASE_PER_LEVEL);
        currentRadius = baseRadius + (abilityStatsData.Level * RANGE_INCREASE_PER_LEVEL);
        currentDamagePerSecond = baseDamagePerSecond + (abilityStatsData.Level * DAMAGE_INCREASE_PER_LEVEL);

        Debug.Log($"IgnitionCodeAbility upgrades applied: Cooldown={currentCooldown}, Duration={currentDuration}, Radius={currentRadius}, DamagePerSecond={currentDamagePerSecond}");
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

        PlayerAnimatorController.Instance?.PlayFireAbilityAnim(abilityAnimationID);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<IgnitionCodeShot>().Initialize(currentRadius, currentDamagePerSecond, currentDuration, enemyLayer);
        Destroy(projectile, projectileLifeTime);
    }
}