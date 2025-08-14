using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Serialized Fields

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private WeaponStats stats;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private Transform weaponModelTransform;
    [SerializeField] private Renderer[] weaponRenderers;
    [SerializeField] private ShootingMode baseMode;

    [Header("Balas")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Disparo")]
    [SerializeField] private float spreadIntensity = 1f;
    [SerializeField] private float spreadAngle = 1f;
    [SerializeField] private int shotgunPellets = 3;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip overheatClip;

    [Header("Configuración de Animación por codigo")]
    [SerializeField] private bool useProceduralAnimations = true;

    [Header("Animation")]
    [SerializeField] private int weaponAnimationID = 0;

    [Header("Sobrecalentamiento")]
    [SerializeField] private float heatPerShot = 10f;
    [SerializeField] private float cooldownRate = 5f;
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float overheatDuration = 2f;
    [SerializeField] private float maxEmissionIntensity = 20f;
    [SerializeField] private float emissionCooldownTime = 0.5f;

    #endregion

    #region Properties and States

    public enum ShootingMode { Single, SemiAuto, Auto }

    public ShootingMode CurrentMode { get; private set; }
    public ShootingMode BaseMode => baseMode;
    public WeaponStats Stats => stats;

    private float currentHeat = 0f;
    public float CurrentHeat => currentHeat;
    public float MaxHeat => maxHeat;
    public bool IsOverheated => isOverheated;

    private float fireRate;
    private float bulletDamage;
    private float timeBetweenShots;

    private bool isOverheated = false;
    private bool isHoldingTrigger = false;
    private Coroutine overheatCoroutine;
    private Coroutine autoFireCoroutine;
    private float nextAllowedShotTime = 0f;
    private Vector3 originalModelPosition;
    private WeaponManager weaponManager;
    private Dictionary<Material, Color> originalEmissionColors = new Dictionary<Material, Color>();
    private Dictionary<Material, bool> originalEmissionEnabled = new Dictionary<Material, bool>();

    private float finalHeatPerShot;
    private float finalCooldownRate;

    private float lastShotTime = 0f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (weaponManager == null) weaponManager = GetComponentInParent<WeaponManager>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (sfxSource == null) sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();
        if (weaponModelTransform != null)
        {
            originalModelPosition = weaponModelTransform.localPosition;
            if (weaponRenderers == null || weaponRenderers.Length == 0)
            {
                weaponRenderers = weaponModelTransform.GetComponentsInChildren<Renderer>();
            }
        }
    }

    private void Start()
    {
        SetupStats();
        StoreOriginalEmissionData();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHeat(weaponAnimationID, currentHeat, maxHeat);
        }
    }

    private void OnEnable()
    {
        if (isOverheated) CancelOverheat();
        DataManager.OnDataChanged += ApplyUpgrades;
    }

    private void OnDisable()
    {
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
        ResetToOriginalEmission();
        DataManager.OnDataChanged -= ApplyUpgrades;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        HandleShootingInput();
        HandleHeat();
    }

    #endregion

    #region Setup & Stats

    private void SetupStats()
    {
        bulletDamage = stats.bulletDamage;
        fireRate = stats.fireRate;
        CurrentMode = stats.shootingMode;
        timeBetweenShots = 1f / fireRate;
        ApplyUpgrades();
    }

    private void ApplyUpgrades()
    {
        const float DAMAGE_INCREASE = 0.1f;
        const float FIRERATE_INCREASE = 0.05f;
        const float COOLDOWN_INCREASE = 0.05f;

        string key = CurrentMode.ToString();

        WeaponStatsData upgradeData = DataManager.GetWeaponStats(key);
        int lvl = upgradeData?.Level ?? 0;

        bulletDamage = stats.bulletDamage * (1 + (upgradeData.Level * DAMAGE_INCREASE));
        fireRate = stats.fireRate * (1 + (upgradeData.Level * FIRERATE_INCREASE));

        finalCooldownRate = cooldownRate * (1 + (upgradeData.Level * COOLDOWN_INCREASE));
        finalHeatPerShot = heatPerShot;

        timeBetweenShots = 1f / fireRate;

        Debug.Log($"Weapon {CurrentMode}: Cooldown Rate = {finalCooldownRate}, Heat per Shot = {finalHeatPerShot}");
    }

    private void StoreOriginalEmissionData()
    {
        if (weaponRenderers != null && weaponRenderers.Length > 0)
        {
            foreach (Renderer renderer in weaponRenderers)
            {
                Material material = renderer.material;
                if (material.HasProperty("_EmissionColor"))
                {
                    originalEmissionColors[material] = material.GetColor("_EmissionColor");
                    originalEmissionEnabled[material] = material.IsKeywordEnabled("_EMISSION");
                }
            }
        }
    }

    #endregion

    #region Input Handling

    private void HandleShootingInput()
    {
        bool triggerPressed = Input.GetKeyDown(KeyCode.Mouse0);
        bool triggerReleased = Input.GetKeyUp(KeyCode.Mouse0);

        if (CurrentMode == ShootingMode.Auto)
        {
            if (triggerPressed) isHoldingTrigger = true;
            if (triggerReleased) isHoldingTrigger = false;

            if (isHoldingTrigger)
            {
                TryShoot();
            }
        }
        else
        {
            if (triggerPressed)
            {
                TryShoot();
            }
        }
    }

    private void TryShoot()
    {
        if (CanShoot())
        {
            Shoot();
            nextAllowedShotTime = Time.time + timeBetweenShots;
            lastShotTime = Time.time;
        }
    }

    #endregion

    #region Heat Management

    private void HandleHeat()
    {
        if (Time.timeScale == 0f) return;

        if (!isOverheated && currentHeat > 0f)
        {
            currentHeat -= finalCooldownRate * Time.deltaTime;
            currentHeat = Mathf.Max(currentHeat, 0f);
        }

        UpdateEmission();

        if (currentHeat >= maxHeat && !isOverheated)
        {
            StartOverheat();
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHeat(weaponAnimationID, currentHeat, maxHeat);
        }
    }

    public void StartOverheat()
    {
        if (isOverheated) return;
        isOverheated = true;
        PlayClip(overheatClip);
        overheatCoroutine = StartCoroutine(OverheatRoutine());
    }

    private IEnumerator OverheatRoutine()
    {
        if (weaponModelTransform != null)
        {
            PlayerAnimatorController.Instance?.PlayRechargeWeaponAnim(weaponAnimationID);

            float t = 0;
            Vector3 downPos = originalModelPosition + new Vector3(0, -0.2f, 0);

            while (t < 1)
            {
                weaponModelTransform.localPosition = Vector3.Lerp(originalModelPosition, downPos, t);
                t += Time.deltaTime / (overheatDuration / 2);
                yield return null;
            }

            float heatStart = currentHeat;
            t = 0;
            while (t < 1)
            {
                currentHeat = Mathf.Lerp(heatStart, 0, t);
                UpdateEmission();
                t += Time.deltaTime / (overheatDuration / 2);
                yield return null;
            }
            currentHeat = 0;
            UpdateEmission();

            t = 0;
            while (t < 1)
            {
                weaponModelTransform.localPosition = Vector3.Lerp(downPos, originalModelPosition, t);
                t += Time.deltaTime / (overheatDuration / 2);
                yield return null;
            }
            weaponModelTransform.localPosition = originalModelPosition;
        }
        else
        {
            yield return new WaitForSeconds(overheatDuration);
            currentHeat = 0;
        }

        isOverheated = false;
    }

    public void CancelOverheat()
    {
        if (!isOverheated) return;
        if (overheatCoroutine != null) StopCoroutine(overheatCoroutine);
        if (weaponModelTransform != null)
        {
            weaponModelTransform.localPosition = originalModelPosition;
        }
        ResetToOriginalEmission();
        isOverheated = false;
        currentHeat = 0;
    }

    #endregion

    #region Shooting Logic

    private bool CanShoot()
    {
        if (isOverheated) return false;
        if (Time.time < nextAllowedShotTime) return false;
        return currentHeat < maxHeat;
    }

    private void Shoot()
    {
        PlayerAnimatorController.Instance?.PlayFireWeaponAnim(weaponAnimationID);

        currentHeat += finalHeatPerShot;
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHeat(weaponAnimationID, currentHeat, maxHeat);
        }

        if (CurrentMode == ShootingMode.SemiAuto)
        {
            ShootShotgun(shotgunPellets);
        }
        else
        {
            ShootBullet(CalculateDirectionAndSpread());
        }

        PlayShotAudio();
        PlayEffect();
    }

    private void ShootBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
        Destroy(bullet, bulletLifetime);
    }

    private void ShootShotgun(int pellets)
    {
        for (int i = 0; i < pellets; i++)
        {
            Vector3 direction = CalculateDirectionAndSpread();
            float lateralOffset = Random.Range(-spreadAngle, spreadAngle);
            Vector3 spawnOffset = bulletSpawnPoint.right * lateralOffset;

            ShootBullet(direction);
        }
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Vector3 direction = playerCamera.transform.forward;
        float angleX = Random.Range(-spreadIntensity, spreadIntensity);
        float angleY = Random.Range(-spreadIntensity, spreadIntensity);
        Quaternion spread = Quaternion.Euler(angleY, angleX, 0);
        return (spread * direction).normalized;
    }

    public void PlayShotAudio() => PlayClip(shootClip);

    #endregion

    #region Effects

    private void PlayEffect()
    {
        if (muzzleEffect != null) muzzleEffect.Play();
    }

    private void UpdateEmission()
    {
        if (weaponRenderers == null || weaponRenderers.Length == 0) return;

        float heatNormalized = currentHeat / maxHeat;
        ApplyEmissionIntensity(heatNormalized * maxEmissionIntensity);
    }

    private void ApplyEmissionIntensity(float intensity)
    {
        foreach (Renderer renderer in weaponRenderers)
        {
            Material material = renderer.material;
            if (!material.HasProperty("_EmissionColor")) continue;

            Color originalColor;
            bool wasOriginallyEmissive;

            if (originalEmissionColors.TryGetValue(material, out originalColor) &&
                originalEmissionEnabled.TryGetValue(material, out wasOriginallyEmissive))
            {
                if (intensity > 0 || wasOriginallyEmissive)
                {
                    material.EnableKeyword("_EMISSION");
                    Color finalColor = originalColor * (1 + intensity);
                    material.SetColor("_EmissionColor", finalColor);
                }
                else
                {
                    if (!wasOriginallyEmissive)
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                    material.SetColor("_EmissionColor", originalColor);
                }
            }
        }
    }

    private void ResetToOriginalEmission()
    {
        if (weaponRenderers == null || weaponRenderers.Length == 0) return;

        foreach (Renderer renderer in weaponRenderers)
        {
            Material material = renderer.material;
            if (!material.HasProperty("_EmissionColor")) continue;

            if (originalEmissionColors.ContainsKey(material) && originalEmissionEnabled.ContainsKey(material))
            {
                if (originalEmissionEnabled[material])
                {
                    material.EnableKeyword("_EMISSION");
                }
                else
                {
                    material.DisableKeyword("_EMISSION");
                }
                material.SetColor("_EmissionColor", originalEmissionColors[material]);
            }
        }
    }

    #endregion

    #region Audio

    private void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    #endregion
}