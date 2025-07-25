using UnityEngine;
using System.Collections;

public class MindjackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject mindjackVFX;

    [Header("Settings")]
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float baseCooldown = 15f;
    [SerializeField] private float baseDamagePerSecond = 20f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseRadius = 5f;

    [Header("Animation")]
    [SerializeField] private int abilityAnimationID = 2;

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

    private IEnumerator Start()
    {
        ApplyUpgrades();
        yield return new WaitForEndOfFrame();

        HUDManager.Instance?.UpdateAbilityUI(gameObject);
        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void OnEnable()
    {
        if (mindjackVFX != null)
        {
            mindjackVFX.SetActive(true);
        }

        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);
    }

    private void OnDisable()
    {
        if (mindjackVFX != null)
        {
            mindjackVFX.SetActive(false);
        }
    }

    private void ApplyUpgrades()
    {
        AbilityStatsData abilityStatsData = DataManager.GetAbilityStats(abilityInfo.abilityName);
        if (abilityStatsData == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f; // Cooldown reduction per level
        const float DAMAGE_INCREASE_PER_LEVEL = 2.0f; // Damage per second increase per level

        currentCooldown = baseCooldown - (abilityStatsData.Level * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDamagePerSecond = baseDamagePerSecond + (abilityStatsData.Level * DAMAGE_INCREASE_PER_LEVEL);

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

        PlayerAnimatorController.Instance?.PlayFireAbilityAnim(abilityAnimationID);

        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<MindjackShot>().Initialize(baseRadius, currentDamagePerSecond, baseDuration);
        Destroy(projectile, projectileLifeTime);
    }
}