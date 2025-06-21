using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Serialized Fields

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private WeaponStats stats;
    [SerializeField] private ShootingMode baseMode;

    [Header("Balas")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Disparo")]
    [SerializeField] private float spreadIntensity = 1f;
    [SerializeField] private float spreadAngle = 1f;
    [SerializeField] private int shotgunPellets = 3;
    [SerializeField] private int minAmmoToInterruptReload = 3;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;

    #endregion

    #region Properties and States

    public enum ShootingMode { Single, SemiAuto, Auto }

    public ShootingMode CurrentMode { get; private set; }
    public ShootingMode BaseMode => baseMode;
    public WeaponStats Stats => stats;
    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;
    public bool IsReloading => isReloading;

    private int maxAmmoPerClip;
    private int currentAmmo;
    private int totalAmmo;
    private float fireRate;
    private float reloadTime;
    private float bulletDamage;
    private float timeBetweenShots;

    private bool isReloading;
    private bool isAutomaticReload;
    private float lastShotTime;
    private Coroutine reloadCoroutine;

    private Coroutine autoFireCoroutine;
    private bool isHoldingTrigger = false;
    private float nextAllowedShotTime = 0f;

    private Transform weaponModelTransform;
    private Vector3 originalModelPosition;
    private int ammoReloadedThisCycle = 0;

    #endregion

    #region Unity Lifecycle

    public void Initialize(Transform modelTransform, Vector3 startPosition)
    {
        this.weaponModelTransform = modelTransform;
        this.originalModelPosition = startPosition;
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

        if (sfxSource == null)
        {
            sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                Debug.LogError("No se encontró el AudioSource para efectos de sonido. Asegúrate de que haya un GameObject llamado 'SFXSource' con un AudioSource en la escena.");
            }
        }
    }

    private void Start()
    {
        SetupStats();
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
    }

    private void OnEnable()
    {
        if (isReloading) CancelReload();
    }

    private void OnDisable()
    {
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleShootingInput();
        HandleReloadInput();
    }

    #endregion

    #region Setup & Stats

    private void SetupStats()
    {
        bulletDamage = stats.bulletDamage;
        fireRate = stats.fireRate;
        reloadTime = stats.reloadTime;
        maxAmmoPerClip = stats.maxAmmoPerClip;
        totalAmmo = stats.totalAmmo;
        currentAmmo = maxAmmoPerClip;

        CurrentMode = stats.shootingMode;

        timeBetweenShots = 1f / fireRate;

        ApplyPassiveUpgrades();
    }

    public void ApplyPassiveUpgrades()
    {
        var upgrades = UpgradeDataStore.Instance;

        bulletDamage *= upgrades.weaponDamageMultiplier;
        fireRate *= upgrades.weaponFireRateMultiplier;
        reloadTime *= upgrades.weaponReloadSpeedMultiplier;
        totalAmmo += upgrades.weaponAmmoBonus;
    }

    #endregion

    #region Input Handling

    private void HandleShootingInput()
    {
        bool triggerHeld = Input.GetKey(KeyCode.Mouse0);
        bool triggerPressed = Input.GetKeyDown(KeyCode.Mouse0);
        bool triggerReleased = Input.GetKeyUp(KeyCode.Mouse0);

        bool triggerPulled = (CurrentMode == ShootingMode.Auto) ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

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
        if (isReloading)
        {
            if (ammoReloadedThisCycle >= minAmmoToInterruptReload)
            {
                CancelReload();
                if (CanShoot()) Shoot();
            }
            return;
        }

        if (CanShoot())
        {
            Shoot();
            nextAllowedShotTime = Time.time + timeBetweenShots;
        }
        else if (currentAmmo <= 0 && totalAmmo > 0 && !isReloading)
        {
            isAutomaticReload = true;
            StartReload();
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmoPerClip && totalAmmo > 0)
        {
            isAutomaticReload = false;
            StartReload();
        }
    }

    #endregion

    #region Shooting Logic

    private bool CanShoot()
    {
        if (isReloading) return false;
        if (Time.time < nextAllowedShotTime) return false;

        int ammoCost = GetAmmoCostPerShot();
        return currentAmmo >= ammoCost;
    }

    private int GetAmmoCostPerShot() => 1;

    private void Shoot()
    {
        currentAmmo --;
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);

        if (CurrentMode == ShootingMode.SemiAuto)
        {
            ShootShotgun(shotgunPellets);
        }
        else
        {
            ShootBullet(CalculateDirectionAndSpread());
        }

        PlayShotAudio();
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

    #region Reloading

    public void StartReload()
    {
        if (isReloading || totalAmmo <= 0 || weaponModelTransform == null) return;
        reloadCoroutine = StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        ammoReloadedThisCycle = 0;
        PlayReloadAudio();

        float animTime = 0.25f;
        Vector3 downPos = originalModelPosition + new Vector3(0, -0.2f, 0);

        float t = 0;
        while (t < animTime)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(originalModelPosition, downPos, t / animTime);
            t += Time.deltaTime;
            yield return null;
        }
        weaponModelTransform.localPosition = downPos;

        int ammoNeeded = maxAmmoPerClip - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);
        float timeForBulletLoop = reloadTime - (2 * animTime);

        if (timeForBulletLoop > 0 && ammoToReload > 0)
        {
            float delayPerBullet = timeForBulletLoop / ammoToReload;
            for (int i = 0; i < ammoToReload; i++)
            {
                yield return new WaitForSeconds(delayPerBullet);
                if (!isReloading) yield break;

                currentAmmo++;
                totalAmmo--;
                ammoReloadedThisCycle++;
                HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            }
        }

        t = 0;
        while (t < animTime)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(downPos, originalModelPosition, t / animTime);
            t += Time.deltaTime;
            yield return null;
        }

        if (weaponModelTransform != null)
        {
            weaponModelTransform.localPosition = originalModelPosition;
        }
        isReloading = false;
        isAutomaticReload = false;
    }

    public void CancelReload()
    {
        if (!isReloading) return;

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }

        if (weaponModelTransform != null)
        {
            weaponModelTransform.localPosition = originalModelPosition;
        }

        isReloading = false;
        isAutomaticReload = false;
    }

    public void PlayReloadAudio() => PlayClip(reloadClip);

    #endregion

    #region Munición

    public bool TryAddAmmo(int amount, out int added)
    {
        int maxReserve = stats.totalAmmo + UpgradeDataStore.Instance.weaponAmmoBonus;
        int spaceLeft = maxReserve - totalAmmo;

        if (spaceLeft <= 0)
        {
            added = 0;
            return false;
        }

        added = Mathf.Min(spaceLeft, amount);
        totalAmmo += added;
        return true;
    }

    #endregion

    #region Audio
    private void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    #endregion
}
