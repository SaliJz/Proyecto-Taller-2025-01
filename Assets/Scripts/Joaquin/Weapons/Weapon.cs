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

    [Header("Animation")]
    [HideInInspector] public Vector3 originalLocalPosition;
    [SerializeField] public Transform weaponModelTransform;

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
        originalLocalPosition = weaponModelTransform.localPosition; // Guarda posición inicial
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

        if (isReadyToShoot && isShooting && !isReloading && currentAmmo > 0)
        {
            burstBulletLeft = bulletPerBurst;
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
        PlayReloadAnimation();
        isReloading = true;

        if (currentShootingMode == ShootingMode.Burst && reloadTime > 0f)
        {
            // Recarga una a una
            while (currentAmmo < maxAmmoPerClip && totalAmmo > 0)
            {
                yield return new WaitForSeconds(reloadTime);

                currentAmmo++;
                totalAmmo--;
                HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
            }
        }
        else
        {
            // Recarga completa normal
            yield return new WaitForSeconds(reloadTime);

            int neededAmmo = maxAmmoPerClip - currentAmmo;
            int ammoToReload = Mathf.Min(neededAmmo, totalAmmo);

            currentAmmo += ammoToReload;
            totalAmmo -= ammoToReload;
            HUDManager.Instance.UpdateAmmo(currentAmmo, totalAmmo);
        }

        isReloading = false;
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

    public void PlayReloadAnimation()
    {
        StartCoroutine(AnimateReload());
    }

    private IEnumerator AnimateReload()
    {
        Vector3 startPos = originalLocalPosition;
        Vector3 downPos = originalLocalPosition + new Vector3(0, -0.2f, 0);
        float speed = 10f;

        // Baja el arma
        float t = 0;
        while (t < 1)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(startPos, downPos, t);
            t += Time.deltaTime * speed;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f); // pausa abajo

        // Sube el arma
        t = 0;
        while (t < 1)
        {
            weaponModelTransform.localPosition = Vector3.Lerp(downPos, startPos, t);
            t += Time.deltaTime * speed;
            yield return null;
        }

        weaponModelTransform.localPosition = startPos;
    }
}
