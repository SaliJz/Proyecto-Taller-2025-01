using UnityEngine;

public class GlitchAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("GlitchTime Settings")]
    [SerializeField] private float baseCooldown = 12f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 50f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private GameObject glitchHandVFX;

    [Header("Animation")]
    [SerializeField] private int abilityAnimationID = 0;

    private float currentCooldown;
    private float currentDuration;

    private bool canUse = true;
    private float currentCooldownTimer = 0;
    private float lastCooldownDisplay = -1f;

    private AbilityInfo abilityInfo;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) Debug.LogError("No se encontró la cámara principal. Asegúrate de que haya una cámara con la etiqueta 'MainCamera' en la escena.");
        }

        if (projectileSpawnPoint == null) Debug.LogError("Projectile Spawn Point no está asignado en GlitchTime.");

        if (projectilePrefab == null) Debug.LogError("Projectile Prefab no está asignado en GlitchTime.");

        if (abilityInfo == null)
        {
            abilityInfo = GetComponent<AbilityInfo>();
            if (abilityInfo == null) Debug.LogError("AbilityInfo no está asignado en GlitchTime.");
        }
    }

    private void Start()
    {
        ApplyUpgrades();
        HUDManager.Instance?.UpdateAbilityUI(gameObject);
        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, 0f, true, currentCooldown);
    }

    private void OnEnable()
    {
        HUDManager.Instance?.UpdateAbilityStatus(abilityInfo.abilityName, currentCooldownTimer, canUse, currentCooldown);
    }

    private void ApplyUpgrades()
    {
        AbilityStats stats = AbilityShopDataManager.GetStats(abilityInfo.abilityName);
        if (stats == null) return;

        const float COOLDOWN_REDUCTION_PER_LEVEL = 1.0f;
        const float DURATION_INCREASE_PER_LEVEL = 0.5f;

        currentCooldown = baseCooldown - (stats.CooldownLevel * COOLDOWN_REDUCTION_PER_LEVEL);
        currentDuration = baseDuration + (stats.DurationLevel * DURATION_INCREASE_PER_LEVEL);

        Debug.Log($"GlitchTime Ability Upgraded: Cooldown={currentCooldown}, Duration={currentDuration}");
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

        projectile.GetComponent<GlitchShot>().Initialize(baseRadius, currentDuration, slowMultiplier, enemyLayer);
        Destroy(projectile, projectileLifeTime);
    }
}