using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MindjackAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    //private const float CenterScreenX = 0.5f;
    //private const float CenterScreenY = 0.5f;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float damagePerSecond = 20f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldown = 15f;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    private bool canUse = true;
    private float currentCooldown = 0;

    private void Start()
    {
        HUDManager.Instance.UpdateAbilityStatus("Ignite", currentCooldown, canUse);
    }

    private void Awake()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera no est� asignada en MindjackAbility.");
        }
        if (projectileSpawnPoint == null)
        {
            Debug.LogError("Projectile Spawn Point no est� asignado en MindjackAbility.");
        }
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab no est� asignado en MindjackAbility.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse)
        {
            ActivateAbility();
        }

        if (!canUse)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0f)
            {
                canUse = true;
            }
        }
    }

    private void ActivateAbility()
    {
        Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)).direction;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

        projectile.GetComponent<MindjackShot>().Initialize(damagePerSecond, duration);
        Destroy(projectile, projectileLifeTime);

        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("Hack", currentCooldown, canUse);
    }
    /*
    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(CenterScreenX, CenterScreenY));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = (targetPoint - projectileSpawnPoint.position).normalized;

        // A�adir dispersi�n
        float spreadX = Random.Range(-spreadIntensity, spreadIntensity);
        float spreadY = Random.Range(-spreadIntensity, spreadIntensity);
        Vector3 spread = playerCamera.transform.right * spreadX + playerCamera.transform.up * spreadY;

        return (direction + spread).normalized;
    }

    private IEnumerator CooldownRoutine()
    {
        canUse = false;
        currentCooldown = cooldown;
        HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse);

        while (currentCooldown > 0)
        {
            yield return new WaitForSeconds(1f);
            currentCooldown -= 1f;
            HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse);
        }

        canUse = true;
        HUDManager.Instance.UpdateAbilityStatus("Mindjack", currentCooldown, canUse);
    }
    */
}