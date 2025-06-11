using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAbilityReceiver : MonoBehaviour
{
    // Referencia a VidaEnemigoGeneral
    private VidaEnemigoGeneral vida;
    private EnemigoRosa enemigoRosa;

    [Header("Attack")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 20f;

    [Header("Slow")]
    [SerializeField] private float baseSpeed = 3f;
    private float currentSpeed;
    public float CurrentSpeed => currentSpeed;

    // Estado de las habilidades
    private bool isMindjacked = false;
    private bool isIgnited = false;
    private float lastAttackTime = 0f;

    // Referencias a las coroutines activas
    private Coroutine slowCoroutine;
    private Coroutine electroHackCoroutine;
    private Coroutine ignitionCoroutine;
    private Coroutine mindjackCoroutine;

    private void Awake()
    {
        vida = GetComponent<VidaEnemigoGeneral>();
        enemigoRosa = GetComponent<EnemigoRosa>();

        if (vida == null)
        {
            Debug.LogWarning("VidaEnemigoGeneral no encontrada en el enemigo.");
        }
        if (enemigoRosa == null)
        {
            Debug.LogWarning("EnemigoRosa no encontrado en el enemigo.");
        }

        // Inicializar velocidad
        currentSpeed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        vida?.RecibirDanio(dmg);
        enemigoRosa?.RecibirDanio(dmg);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        currentSpeed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;
    }

    public void ApplyElectroHack(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
    {
        if (electroHackCoroutine != null)
        {
            StopCoroutine(electroHackCoroutine);
        }
        electroHackCoroutine = StartCoroutine(ElectroHackRoutine(tickDamage, tickInterval, ticks, slowMultiplier));
    }

    private IEnumerator ElectroHackRoutine(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
    {
        ApplySlow(slowMultiplier, tickInterval * ticks);

        for (int i = 0; i < ticks; i++)
        {
            if (vida == null || vida.vida <= 0 || enemigoRosa == null || enemigoRosa.vida <= 0) yield break;

            TakeDamage(tickDamage);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        if (isIgnited) return;

        if (ignitionCoroutine != null)
        {
            StopCoroutine(ignitionCoroutine);
        }
        ignitionCoroutine = StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
    }

    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
    {
        isIgnited = true;
        float elapsed = 0f;

        while (elapsed + 1f <= duration)
        {
            TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        float remaining = duration - elapsed;

        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
            TakeDamage(damagePerSecond * remaining);
        }

        isIgnited = false;
        ignitionCoroutine = null;
    }

    public void ApplyMindjack(float damagePerSecond, float duration)
    {
        if (isMindjacked) return;
        isMindjacked = true;

        if (mindjackCoroutine != null)
        {
            StopCoroutine(mindjackCoroutine);
        }
        mindjackCoroutine = StartCoroutine(MindjackRoutine(damagePerSecond, duration));
    }

    private IEnumerator MindjackRoutine(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        float damageInterval = 1f;

        while (elapsed < duration && vida.vida > 0)
        {
            GameObject target = FindNearestEnemy();
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                // Movimiento hacia el enemigo
                if (distance > 1.5f) // separación mínima
                {
                    transform.LookAt(target.transform);
                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, currentSpeed * Time.deltaTime);
                }

                // Ataque si está cerca
                if (distance <= 2f && Time.time >= lastAttackTime + attackCooldown)
                {
                    EnemyAbilityReceiver other = target.GetComponent<EnemyAbilityReceiver>();
                    if (other != null && !other.isMindjacked)
                    {
                        other.TakeDamage(contactDamage);
                        lastAttackTime = Time.time;
                    }
                }
            }

            TakeDamage(damagePerSecond); // daño por segundo
            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
        }

        isMindjacked = false;
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


