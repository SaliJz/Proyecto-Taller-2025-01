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
    [SerializeField] private Transform weaponModelTransform;
    [SerializeField] private WeaponStats stats;
    [SerializeField] private ShootingMode baseMode;

    [Header("Balas")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Disparo")]
    [SerializeField] private float spreadIntensity = 1f;
    [SerializeField] private float spreadAngle = 1f;
    [SerializeField] private int shotgunPellets = 3;

    #endregion

    #region Propiedades y Estados

    public enum ShootingMode { Single, SemiAuto, Auto }

    public ShootingMode CurrentMode { get; private set; }
    public ShootingMode BaseMode => baseMode;
    public WeaponStats Stats => stats;
    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;
    public Transform WeaponModelTransform => weaponModelTransform;
    public bool IsReloading => isReloading;
    public Vector3 OriginalLocalPosition => originalLocalPosition;

    private int maxAmmoPerClip;
    private int currentAmmo;
    private int totalAmmo;
    private float fireRate;
    private float reloadTime;
    private float bulletDamage;

    private bool isReloading;
    private bool isAutomaticReload;
    private float lastShotTime;
    private Coroutine reloadCoroutine;
    private Coroutine reloadAnimCoroutine;
    private Vector3 originalLocalPosition;

    private bool animationInProgress;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        playerCamera ??= Camera.main;
        originalLocalPosition = weaponModelTransform.localPosition;
    }

    private void Start()
    {
        SetupStats();
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
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
        bool triggerPulled = false;

        switch (CurrentMode)
        {
            case ShootingMode.Single:
            case ShootingMode.SemiAuto:
                triggerPulled = Input.GetKeyDown(KeyCode.Mouse0);
                break;
            case ShootingMode.Auto:
                triggerPulled = Input.GetKey(KeyCode.Mouse0);
                break;
        }

        if (triggerPulled)
        {
            if (isReloading && !isAutomaticReload && currentAmmo > 0 && animationInProgress)
            {
                CancelReload();
            }

            if (CanShoot())
            {
                lastShotTime = Time.time;
                Shoot();
            }
            else if (currentAmmo <= 0 && totalAmmo > 0 && !isReloading)
            {
                isAutomaticReload = true;
                reloadCoroutine = StartCoroutine(Reload());
            }
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmoPerClip && totalAmmo > 0)
        {
            isAutomaticReload = false;
            reloadCoroutine = StartCoroutine(Reload());
        }
    }

    #endregion

    #region Shooting Logic

    private bool CanShoot()
    {
        if (isReloading) return false;
        if (Time.time < lastShotTime + 1f / fireRate) return false;

        int ammoCost = GetAmmoCostPerShot();
        return currentAmmo >= ammoCost;
    }

    private int GetAmmoCostPerShot() => 1;

    private void Shoot()
    {
        if (Time.timeScale == 0) return;

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
    }

    private void ShootBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
        bullet.GetComponent<Bullet>().Initialize(bulletDamage);
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

    #endregion

    #region Reloading

    private IEnumerator Reload()
    {
        isReloading = true;
        animationInProgress = true;

        int ammoNeeded = maxAmmoPerClip - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);
        float totalDuration = reloadTime;
        float delayPerBullet = reloadTime / ammoNeeded;

        Coroutine anim = StartCoroutine(ReloadAnimation(totalDuration));

        for (int i = 0; i < ammoToReload; i++)
        {
            if (!isReloading) yield break;

            currentAmmo++;
            totalAmmo--;
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            yield return new WaitForSeconds(delayPerBullet);
        }

        isReloading = false;
        isAutomaticReload = false;
        animationInProgress = false;
    }

    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        if (reloadAnimCoroutine != null)
        {
            StopCoroutine(reloadAnimCoroutine);
            reloadAnimCoroutine = null;
        }

        if (weaponModelTransform.gameObject != null)
        {
            weaponModelTransform.localPosition = originalLocalPosition;
        }

        isReloading = false;
        isAutomaticReload = false;
        animationInProgress = false;
    }

    private IEnumerator ReloadAnimation(float duration)
    {
        Vector3 downPos = originalLocalPosition + new Vector3(0, -0.2f, 0);
        float half = duration / 2f;
        float t = 0f;

        while (t < 1f)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(originalLocalPosition, downPos, t);
            t += Time.deltaTime / half;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(downPos, originalLocalPosition, t);
            t += Time.deltaTime / half;
            yield return null;
        }

        weaponModelTransform.localPosition = originalLocalPosition;
    }

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
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
        return true;
    }

    #endregion
}
