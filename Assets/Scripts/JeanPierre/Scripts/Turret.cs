using System.Collections;
using UnityEngine;

public enum TurretType { Slow, Fast }

[System.Serializable]
public class TurretStats
{
    public float rotationSpeed;
    public float fireRate;
    public float range;
    public int damage;
    public float chargeTime;
    public GameObject projectilePrefab;
    public GameObject muzzleFlashPrefab;
}

public class Turret : MonoBehaviour
{
    [Header("Turret Configuration")]
    public TurretType turretType = TurretType.Slow;

    [Header("Turret Parts")]
    public Transform baseTransform;
    public Transform turretHead;
    public Transform[] firePoints;

    [Header("Turret Stats")]
    public TurretStats slowTurretStats;
    public TurretStats fastTurretStats;

    [Header("Target Settings")]
    public LayerMask playerLayer = 1;
    public string playerTag = "Player";

    [Header("Projectile Settings")]
    public float projectileSpeed = 10f;

    [Header("Rotation Settings")]
    public float baseRotationOffset = 0f;

    private Transform player;
    private float nextFireTime;
    private TurretStats currentStats;
    private int currentFirePointIndex = 0;
    private float currentTurretHeadAngle = 0f;
    private GameObject muzzleEffectInstance;
    private bool isShooting = false;

    private const float MAX_TURRET_ANGLE = 30f;
    private const float MIN_TURRET_ANGLE = -30f;

    void Start()
    {
        currentStats = (turretType == TurretType.Slow) ? slowTurretStats : fastTurretStats;

        var playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (playerGO)
            player = playerGO.transform;
        else
            Debug.LogWarning($"No se encontró Player en {name}");

        if (baseTransform == null || turretHead == null || firePoints == null || firePoints.Length == 0)
        {
            Debug.LogError("Faltan referencias en la torreta: " + gameObject.name);
        }

        if (currentStats.muzzleFlashPrefab != null && firePoints != null && firePoints.Length > 0)
        {
            muzzleEffectInstance = Instantiate(
                currentStats.muzzleFlashPrefab,
                firePoints[0].position,
                firePoints[0].rotation,
                firePoints[0]
            );
            muzzleEffectInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= currentStats.range)
        {
            RotateToTarget();

            if (CanShoot() && !isShooting)
            {
                StartCoroutine(ChargeAndShoot());
            }
        }
    }

    void RotateToTarget()
    {
        if (player == null || baseTransform == null || turretHead == null)
            return;

        Vector3 targetDirection = player.position - transform.position;

        Vector3 baseDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);
        if (baseDirection != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(baseDirection);
            baseRotation *= Quaternion.Euler(0, baseRotationOffset, 0); 

            baseTransform.rotation = Quaternion.Slerp(
                baseTransform.rotation,
                Quaternion.Euler(0, baseRotation.eulerAngles.y, 0), 
                currentStats.rotationSpeed * Time.deltaTime
            );
        }

        float horizontalDistance = baseDirection.magnitude;
        float targetVerticalAngle = Mathf.Atan2(targetDirection.y, horizontalDistance) * Mathf.Rad2Deg;
        targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, MIN_TURRET_ANGLE, MAX_TURRET_ANGLE);

        turretHead.localRotation = Quaternion.Slerp(
            turretHead.localRotation,
            Quaternion.Euler(0, 0, targetVerticalAngle), 
            currentStats.rotationSpeed * Time.deltaTime
        );
    }

    bool CanShoot()
    {
        return Time.time >= nextFireTime && IsTargetInLineOfSight();
    }

    bool IsTargetInLineOfSight()
    {
        if (player == null || firePoints == null || firePoints.Length == 0)
            return false;

        Transform currentFirePoint = firePoints[currentFirePointIndex];
        if (currentFirePoint == null)
            return false;

        Vector3 directionToTarget = (player.position - currentFirePoint.position).normalized;

        if (Physics.Raycast(currentFirePoint.position, directionToTarget, out RaycastHit hit, currentStats.range))
        {
            return hit.transform.CompareTag(playerTag);
        }

        return false;
    }

    IEnumerator ChargeAndShoot()
    {
        isShooting = true;

        if (currentStats.projectilePrefab == null || firePoints == null || firePoints.Length == 0)
        {
            isShooting = false;
            yield break;
        }

        Transform currentFirePoint = firePoints[currentFirePointIndex];
        if (currentFirePoint == null)
        {
            isShooting = false;
            yield break;
        }

        GameObject projectile = Instantiate(
            currentStats.projectilePrefab,
            currentFirePoint.position,
            currentFirePoint.rotation
        );

        var projectileScript = projectile.GetComponent<TurretProjectile>();
        if (projectileScript != null)
        {
            projectileScript.player = player;
            projectileScript.speed = projectileSpeed;
            projectileScript.damage = currentStats.damage;
            projectileScript.chargeTime = currentStats.chargeTime;
            projectileScript.firePoint = currentFirePoint; 
        }

        if (muzzleEffectInstance != null)
        {
            muzzleEffectInstance.transform.SetParent(currentFirePoint);
            muzzleEffectInstance.transform.localPosition = Vector3.zero;
            muzzleEffectInstance.transform.localRotation = Quaternion.identity;
            muzzleEffectInstance.SetActive(true);
            StartCoroutine(DisableMuzzleFlash());
        }

        currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
        nextFireTime = Time.time + (1f / currentStats.fireRate);

        yield return new WaitForSeconds(currentStats.chargeTime);

        isShooting = false;
    }

    IEnumerator DisableMuzzleFlash()
    {
        yield return new WaitForSeconds(0.1f);
        muzzleEffectInstance?.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float rangeToShow = (currentStats != null) ? currentStats.range : (turretType == TurretType.Slow ? slowTurretStats.range : fastTurretStats.range);
        Gizmos.DrawWireSphere(transform.position, rangeToShow);

        if (firePoints != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < firePoints.Length; i++)
            {
                if (firePoints[i] != null)
                {
                    Gizmos.DrawWireSphere(firePoints[i].position, 0.2f);
                    if (i == currentFirePointIndex)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(firePoints[i].position, 0.1f);
                        Gizmos.color = Color.yellow;
                    }
                }
            }
        }

        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}