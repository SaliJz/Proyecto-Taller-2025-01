using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAbilityReceiver : MonoBehaviour
{
    private VidaEnemigoGeneral enemyHealth;
    private EnemigoRosa pinkEnemyHealth;
    /*
    [Header("Attack")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 20f;
    */
    [Header("Slow")]
    [SerializeField] private float baseSpeed = 3f;
    private float currentSpeed;
    public float CurrentSpeed => currentSpeed;

    private bool isMindjacked = false;
    private bool isIgnited = false;
    private bool isElectroHacked = false;
    private bool isSlowed = false;

    private Coroutine slowCoroutine;
    private Coroutine electroHackCoroutine;
    private Coroutine ignitionCoroutine;
    private Coroutine mindjackCoroutine;

    private void Awake()
    {
        enemyHealth = GetComponent<VidaEnemigoGeneral>();
        pinkEnemyHealth = GetComponent<EnemigoRosa>();

        if (enemyHealth == null)
        {
            Debug.LogWarning("VidaEnemigoGeneral no encontrada en el enemigo.");
        }
        if (pinkEnemyHealth == null)
        {
            Debug.LogWarning("EnemigoRosa no encontrado en el enemigo.");
        }

        currentSpeed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        enemyHealth?.RecibirDanio(dmg);
        pinkEnemyHealth?.RecibirDanio(dmg);
    }

    public void ApplySlow(float multiplier, float duration, GameObject effectToDestroy)
    {
        if (isSlowed) return;

        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(multiplier, duration, effectToDestroy));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration, GameObject effectToDestroy)
    {
        isSlowed = true;

        currentSpeed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;

        if (effectToDestroy != null)
        {
            Destroy(effectToDestroy);
        }

        isSlowed = false;
        slowCoroutine = null;
    }

    public void ApplyGlitchTime(float slowMultiplier, float duration, GameObject particlePrefab)
    {
        GameObject particleInstance = null;
        if (particlePrefab != null)
        {
            particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
        }

        ApplySlow(slowMultiplier, duration, particleInstance);
    }

    public void ApplyElectroHack(float damagePerSecond, float duration, float slowMultiplier)
    {
        if (isElectroHacked) return;

        if (electroHackCoroutine != null) StopCoroutine(electroHackCoroutine);
        electroHackCoroutine = StartCoroutine(ElectroHackRoutine(damagePerSecond, duration, slowMultiplier));
    }

    private IEnumerator ElectroHackRoutine(float damagePerSecond, float duration, float slowMultiplier)
    {
        isElectroHacked = true;

        ApplySlow(slowMultiplier, duration, null);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isElectroHacked = false;
        electroHackCoroutine = null;
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        if (isIgnited) return;

        if (ignitionCoroutine != null) StopCoroutine(ignitionCoroutine);
        ignitionCoroutine = StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
    }

    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
    {
        isIgnited = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isIgnited = false;
        ignitionCoroutine = null;
    }

    public void ApplyMindjack(float damagePerSecond, float duration)
    {
        if (isMindjacked) return;

        Debug.Log("[En proceso] Aplicando Mindjack a " + gameObject.name);

        if (mindjackCoroutine != null) StopCoroutine(mindjackCoroutine);
        mindjackCoroutine = StartCoroutine(MindjackRoutine(damagePerSecond, duration));
    }

    private IEnumerator MindjackRoutine(float damagePerSecond, float duration)
    {
        isMindjacked = true;

        float elapsed = 0f;
        float damageInterval = 1f;

        while (elapsed < duration && enemyHealth.vida > 0)
        {
            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
        }

        isMindjacked = false;
        mindjackCoroutine = null;
    }
    /*
    private GameObject FindNearestEnemy()
    {
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (var col in hits)
        {
            if (col.gameObject != gameObject)
            {
                EnemyAbilityReceiver other = col.GetComponent<EnemyAbilityReceiver>();
                if (other != null && !other.isMindjacked)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearest = col.gameObject;
                    }
                }
            }
        }

        return nearest;
    }
    */
}