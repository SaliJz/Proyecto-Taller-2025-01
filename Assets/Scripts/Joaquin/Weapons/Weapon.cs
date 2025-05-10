using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("Shoot")]
    [SerializeField] bool isShooting;
    [SerializeField] private float lastShotTime = 0f;
    [SerializeField] private float fireRate = 0.5f; // Disparos por segundo

    [Header("Ammo")]
    [SerializeField] private int maxAmmoPerClip = 30;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int totalAmmo = 90;
    [SerializeField] private float reloadTime = 1.5f;

    public bool isReloading = false;
    private bool isAutomaticReload = false;
    private bool animationInProgress = false;

    [Header("Shotgun")]
    [SerializeField] private int bulletPerBurst = 3;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity = 1f; // Intensidad de la propagaci�n
    [SerializeField] private float spreadAngle = 1f; // Grados de propagaci�n

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletDamage = 10;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 2f; // Tiempos de vida de la bala en segundos

    [SerializeField] private WeaponStats stats;

    [Header("Animation")]
    [HideInInspector] public Vector3 originalLocalPosition;
    [SerializeField] public Transform weaponModelTransform;
    private Coroutine reloadCoroutine;

    private float baseBulletDamage;
    private float baseFireRate;
    private float baseReloadTime;
    private int baseTotalAmmo;

    public WeaponStats Stats => stats;

    public enum ShootingMode
    {
        Single, // Pistola
        SemiAuto, // Escopeta
        Auto // Rifle
    }

    public ShootingMode currentShootingMode;
    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;

    // Inicializa el arma al activarse
    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        originalLocalPosition = weaponModelTransform.localPosition;
    }

    // Inicializa el arma
    private void Start()
    {
        // Obtener los valores base desde WeaponStats (ScriptableObject)
        baseBulletDamage = stats.bulletDamage;
        baseFireRate = stats.fireRate;
        baseReloadTime = stats.reloadTime;
        baseTotalAmmo = stats.totalAmmo;

        // Aplicar upgrades usando los valores base
        ApplyPassiveUpgrades();

        // Inicializar variables que no cambian por pasiva
        currentAmmo = stats.maxAmmoPerClip;
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
    }

    // Actualiza el estado del arma
    private void Update()
    {
        // Entrada de disparo
        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        // Disparo
        if (isShooting)
        {
            // Si est� recargando manualmente y no es recarga autom�tica, la interrumpe
            if (isReloading && !isAutomaticReload)
            {
                if (!animationInProgress) CancelReload(); // // Espera a que termine animaci�n actual
            }

            if (CanShoot())
            {
                lastShotTime = Time.time;
                Shoot();
            }
            // Recarga autom�tica si no hay balas
            else if (currentAmmo <= 0 && totalAmmo > 0 && !isReloading)
            {
                isAutomaticReload = true;
                reloadCoroutine = StartCoroutine(Reload());
            }
        }

        // Recarga manual
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmoPerClip && totalAmmo > 0)
        {
            isAutomaticReload = false;
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }
    }

    // Verifica si el arma puede disparar
    private bool CanShoot()
    {
        if (isReloading || Time.time < lastShotTime + 1f / fireRate)
            return false; // no puede disparar si est� recargando o no ha pasado el tiempo de recarga

        if (currentShootingMode == ShootingMode.SemiAuto)
            return currentAmmo > 0; // permite disparar si hay al menos 1 bala

        return currentAmmo >= GetAmmoCostPerShot();
    }

    // Calcula el costo de munici�n por disparo
    private int GetAmmoCostPerShot()
    {
        return currentShootingMode == ShootingMode.SemiAuto ? bulletPerBurst : 1;
    }

    // Dispara el arma
    private void Shoot()
    {
        if (Time.timeScale == 0f) return; // Evita disparar si el tiempo est� pausado

        if (currentShootingMode == ShootingMode.SemiAuto)
        {
            int bulletsToShoot = Mathf.Min(currentAmmo, bulletPerBurst);
            currentAmmo -= bulletsToShoot;
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            ShootShotgun(bulletsToShoot);
        }
        else
        {
            int ammoCost = GetAmmoCostPerShot();
            if (currentAmmo < ammoCost) return;

            currentAmmo -= ammoCost;
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            ShootSingle();
        }
    }

    // Dispara en modo "Single" o "Auto"
    private void ShootSingle()
    {
        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
        bullet.GetComponent<Bullet>().Initialize(bulletDamage);
        Destroy(bullet, bulletLifetime);
    }

    // Dispara en modo "Shotgun"
    private void ShootShotgun(int pellets)
    {
        for (int i = 0; i < pellets; i++)
        {
            Vector3 direction = CalculateDirectionAndSpread().normalized;

            // Desplazamiento lateral leve al spawn 
            float lateralOffset = Random.Range(-spreadAngle, spreadAngle); 
            Vector3 spawnOffset = bulletSpawnPoint.right * lateralOffset;

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position + spawnOffset, Quaternion.LookRotation(direction));
            bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
            bullet.GetComponent<Bullet>().Initialize(bulletDamage);
            Destroy(bullet, bulletLifetime);
        }
    }

    // Recarga el arma
    private IEnumerator Reload()
    {
        isReloading = true;
        animationInProgress = true;

        int neededAmmo = maxAmmoPerClip - currentAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, totalAmmo);

        Coroutine animCoroutine = StartCoroutine(AnimateReload(reloadTime));

        if (currentShootingMode == ShootingMode.SemiAuto && reloadTime > 0f)
        {
            float timePerBullet = reloadTime / ammoToReload;

            for (int i = 0; i < ammoToReload; i++)
            {
                currentAmmo++;
                totalAmmo--;
                HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
                yield return new WaitForSeconds(timePerBullet);
            }
        }
        else
        {
            yield return new WaitForSeconds(reloadTime);

            currentAmmo += ammoToReload;
            totalAmmo -= ammoToReload;
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
        }

        isReloading = false;
        isAutomaticReload = false;
    }

    // Cancela la recarga
    public void CancelReload()
    {
        if (isReloading && reloadCoroutine != null)
        {
            isReloading = false; 
            isAutomaticReload = false;
        }
    }

    // Calcula la direcci�n y la propagaci�n de la bala
    private Vector3 CalculateDirectionAndSpread()
    {
        // Direcci�n de la c�mara al centro de pantalla
        Vector3 direction = playerCamera.transform.forward;

        // Genera un peque�o �ngulo de desviaci�n
        float angleX = Random.Range(-spreadIntensity, spreadIntensity);
        float angleY = Random.Range(-spreadIntensity, spreadIntensity);

        // Aplica la desviaci�n en la rotaci�n
        Quaternion spreadRotation = Quaternion.Euler(angleY, angleX, 0);
        Vector3 spreadDirection = spreadRotation * direction;

        return spreadDirection.normalized;
    }

    // Reproduce la animaci�n de recarga
    public void PlayReloadAnimation()
    {
        Debug.Log("Recargando...");
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }

        reloadCoroutine = StartCoroutine(AnimateReload(reloadTime));
    }

    // Animaci�n de recarga
    private IEnumerator AnimateReload(float totalTime)
    {
        Vector3 startPos = originalLocalPosition;
        Vector3 downPos = originalLocalPosition + new Vector3(0, -0.2f, 0);
        float halfTime = totalTime / 2f;

        float t = 0f;
        while (t < 1f)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(startPos, downPos, t);
            t += Time.deltaTime / halfTime;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(downPos, startPos, t);
            t += Time.deltaTime / halfTime;
            yield return null;
        }

        weaponModelTransform.localPosition = startPos;
    }

    // Intenta a�adir munici�n al arma
    public bool TryAddAmmo(int amountToAdd, out int amountActuallyAdded)
    {
        Debug.Log($"Intentando a�adir {amountToAdd} de munici�n. Munici�n actual: {currentAmmo} / {totalAmmo}");
        int maxReserveAmmo = baseTotalAmmo + UpgradeDataStore.Instance.weaponAmmoBonus;

        int availableSpace = maxReserveAmmo - totalAmmo;

        if (availableSpace <= 0)
        {
            amountActuallyAdded = 0;
            Debug.Log("No hay espacio disponible para a�adir munici�n.");
            return false;
        }

        amountActuallyAdded = Mathf.Min(amountToAdd, availableSpace);
        totalAmmo += amountActuallyAdded;
        Debug.Log($"A�adido {amountActuallyAdded} de munici�n. Munici�n total: {totalAmmo}");
        return true;
    }

    // Aplica las mejoras pasivas al arma
    public void ApplyPassiveUpgrades()
    {
        var data = UpgradeDataStore.Instance;

        bulletDamage = baseBulletDamage * data.weaponDamageMultiplier;
        fireRate = baseFireRate + data.weaponFireRateMultiplier;
        reloadTime = baseReloadTime * data.weaponReloadSpeedMultiplier;
        totalAmmo = baseTotalAmmo + data.weaponAmmoBonus;
    }
}
