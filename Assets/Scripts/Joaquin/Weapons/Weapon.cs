using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("Shoot")]
    [SerializeField] bool isShooting;
    [SerializeField] bool isReadyToShoot;
    bool allowReset = true;
    [SerializeField] private float shootingDelay;

    [Header("Ammo")]
    [SerializeField] private int maxAmmoPerClip = 30;
    public int currentAmmo;
    public int totalAmmo = 90;
    [SerializeField] private float reloadTime = 1.5f;

    private bool isReloading = false;

    [Header("Burst")]
    [SerializeField] private int bulletPerBurst = 3;
    [SerializeField] private int burstBulletLeft;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 2f; // Tiempos de vida de la bala en segundos

    [SerializeField] private WeaponStats stats;

    public WeaponStats Stats => stats;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        isReadyToShoot = true;
        burstBulletLeft = bulletPerBurst;
    }

    private void Start()
    {
        currentAmmo = stats.maxAmmoPerClip;
        totalAmmo = stats.totalAmmo;
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
    }

    private void Update()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0); // Disparo al mantener el botón
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0); // Disparo al presionar el botón
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmoPerClip && totalAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (isReadyToShoot && isShooting)
        {
            burstBulletLeft = bulletPerBurst;

            if (isReloading || currentAmmo <= 0) return;
            Shoot();
        }
    }

    private void Shoot()
    {
        currentAmmo--;
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);

        isReadyToShoot = false;

        // Obtiene la dirección de disparo
        Vector3 shootDirection = CalculateDirectionAndSpread().normalized;

        // Instancia la bala
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(shootDirection));

        bullet.transform.forward = shootDirection; // Asegura que la bala apunte en la dirección correcta

        // Obtiene el Rigidbody de la bala y le aplica una fuerza
        bullet.GetComponent<Rigidbody>().AddForce(shootDirection * bulletSpeed, ForceMode.Impulse);

        // Destruye la bala después de un tiempo
        Destroy(bullet, bulletLifetime);

        if (allowReset)
        {
            Invoke("ResetShoot", shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == ShootingMode.Burst && burstBulletLeft > 1)
        {
            burstBulletLeft--;
            Invoke("Shoot", shootingDelay);
        }
    }

    private void ResetShoot()
    {
        isReadyToShoot = true;
        allowReset = true;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = maxAmmoPerClip - currentAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, totalAmmo);

        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        isReloading = false;
        HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Centro de la pantalla
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100); // Si no hay colisión, apunta a 100 unidades de distancia
        }

        Vector3 direction = targetPoint - bulletSpawnPoint.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float Y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // Retorna la dirección con la propagación aplicada
        return direction + new Vector3(x, Y, 0);
    }
}
