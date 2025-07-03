using System.Collections;
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
    [SerializeField] private ShootingMode baseMode;
    [SerializeField] private Transform weaponModelTransform;

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

    [Header("Configuraci�n de Animaci�n por codigo")]
    [SerializeField] private bool useProceduralAnimations = true;

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
    private bool isHoldingTrigger = false;
    private Coroutine reloadCoroutine;
    private Coroutine autoFireCoroutine;
    private float nextAllowedShotTime = 0f;
    private Vector3 originalModelPosition;
    private int ammoReloadedThisCycle = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (sfxSource == null) sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();
        if (weaponModelTransform != null)
        {
            originalModelPosition = weaponModelTransform.localPosition;
        }

        SetupStats();
    }

    private void Start()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
        }
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

        Debug.Log("mejoras");
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
            StartReload();
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmoPerClip && totalAmmo > 0)
        {
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
        PlayEffect();
    }

    private void ShootBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Rigidbody>().linearVelocity = direction * bulletSpeed;
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

        int ammoNeeded = maxAmmoPerClip - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);

        if (ammoToReload <= 0)
        {
            isReloading = false;
            yield break;
        }

        float animDownTime = useProceduralAnimations ? 0.2f : 0f;
        float animUpTime = useProceduralAnimations ? 0.2f : 0f;

        // Animaci�n Hacia Abajo 
        if (useProceduralAnimations && weaponModelTransform != null)
        {
            Vector3 downPos = originalModelPosition + new Vector3(0, -0.2f, 0);
            float t = 0;
            while (t < animDownTime)
            {
                weaponModelTransform.localPosition = Vector3.Lerp(originalModelPosition, downPos, t / animDownTime);
                t += Time.deltaTime;
                yield return null;
            }
            weaponModelTransform.localPosition = downPos;
        }

        // L�gica de Recarga por Bala 
        float timeForBulletLoop = stats.reloadTime - (animDownTime + animUpTime);
        if (timeForBulletLoop < 0) timeForBulletLoop = 0;
        float delayPerBullet = timeForBulletLoop / ammoToReload;

        for (int i = 0; i < ammoToReload; i++)
        {
            if (delayPerBullet > 0.01f)
            {
                yield return new WaitForSeconds(delayPerBullet);
            }

            if (!isReloading) yield break;

            currentAmmo++;
            totalAmmo--;
            ammoReloadedThisCycle++;
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            }
        }

        // Si el delay era muy corto, esperar el tiempo restante de golpe
        if (delayPerBullet <= 0.01f && timeForBulletLoop > 0)
        {
            yield return new WaitForSeconds(timeForBulletLoop);
        }

        // Animaci�n Hacia Arriba 
        if (useProceduralAnimations && weaponModelTransform != null)
        {
            Vector3 downPos = originalModelPosition + new Vector3(0, -0.2f, 0);
            float t = 0;
            while (t < animUpTime)
            {
                weaponModelTransform.localPosition = Vector3.Lerp(downPos, originalModelPosition, t / animUpTime);
                t += Time.deltaTime;
                yield return null;
            }
            weaponModelTransform.localPosition = originalModelPosition;
        }

        isReloading = false;
    }

    public void CancelReload()
    {
        if (!isReloading) return;
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);

        if (weaponModelTransform != null)
        {
            weaponModelTransform.localPosition = originalModelPosition;
        }

        isReloading = false;
    }

    public void PlayReloadAudio() => PlayClip(reloadClip);

    #endregion

    #region Munici�n

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
