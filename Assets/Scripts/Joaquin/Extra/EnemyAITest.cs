using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(Collider))] // Asegúrate de que el objeto tenga un Collider
public class EnemyAITest : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float life = 100f;
    [SerializeField] private int fragments = 50; // Fragmentos de información que suelta el enemigo
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 20f;

    [SerializeField] private float currentSpeed;
    private bool isMindjacked = false;
    private bool isIgnited = false;
    private float lastAttackTime = 0f;

    [Header("Glow")]
    [SerializeField] private Renderer meshRenderer; // arrastrar el hijo con Mesh Renderer
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material mindjackMaterial;

    // Referencias a las coroutines activas
    private Coroutine slowCoroutine;
    private Coroutine electroHackCoroutine;
    private Coroutine ignitionCoroutine;
    private Coroutine mindjackCoroutine;

    private Transform player;
    private bool isDead = false;

    private void Start()
    {
        currentSpeed = baseSpeed;
        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    private void Die()
    {
        if (isDead) return; // Protege de múltiples ejecuciones
        isDead = true;

        Destroy(gameObject); // Destruye el enemigo al morir
        HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragmentos de información
        FindObjectOfType<MissionManager>().RegisterKill(gameObject.tag); // Actualiza la misión
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return; // Ya muerto, no recibir más daño

        life -= dmg; // Resta el daño a la vida del enemigo
        if (life <= 0)
        {
            Die();
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        // Detener la coroutine anterior si está activa
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        StartCoroutine(SlowRoutine(multiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        currentSpeed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;
    }

    public void ApplyElectroHack(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
    {
        // Detener la coroutine anterior si está activa
        if (electroHackCoroutine != null)
        {
            StopCoroutine(electroHackCoroutine);
        }

        StartCoroutine(ElectroHackRoutine(tickDamage, tickInterval, ticks, slowMultiplier));
    }

    private IEnumerator ElectroHackRoutine(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
    {
        ApplySlow(slowMultiplier, tickInterval * ticks);

        for (int i = 0; i < ticks; i++)
        {
            TakeDamage(tickDamage);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        if (isIgnited) return;
        isIgnited = true;

        // Detener la coroutine anterior si está activa
        if (ignitionCoroutine != null)
        {
            StopCoroutine(ignitionCoroutine);
        }

        StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
    }

    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
    {
        int ticks = Mathf.FloorToInt(duration);
        for (int i = 0; i < ticks; i++)
        {
            TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
    }

    public void ApplyMindjack(float damagePerSecond, float duration)
    {
        // Detener la coroutine anterior si está activa
        if (isMindjacked) return;
        isMindjacked = true;
        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>(); // Obtener la instancia de la habilidad
        if (mindjackAbility != null)
        {
            mindjackAbility.EnemyMindjacked(true); // Actualizar el estado de la habilidad
        }
        else
        {
            Debug.LogError("No se encontró la habilidad MindjackAbility.");
        }

        // Detener la coroutine anterior si está activa
        if (mindjackCoroutine != null)
        {
            StopCoroutine(mindjackCoroutine);
        }

        StartCoroutine(MindjackRoutine(damagePerSecond, duration));

        if (meshRenderer != null && mindjackMaterial != null)
        {
            meshRenderer.material = mindjackMaterial;
        }
    }

    private IEnumerator MindjackRoutine(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        float damageInterval = 1f;

        while (elapsed < duration && life > 0)
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
                    EnemyAITest other = target.GetComponent<EnemyAITest>();
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

        // Fin del efecto Mindjack
        isMindjacked = false;

        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>(); // Obtener la instancia de la habilidad
        if (mindjackAbility != null)
        {
            mindjackAbility.EnemyMindjacked(false); // Restablecer el estado de la habilidad
        }
        else
        {
            Debug.LogError("No se encontró la habilidad MindjackAbility.");
        }

        // Restaurar material si es necesario
        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
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
                EnemyAITest other = col.GetComponent<EnemyAITest>();
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

    private void OnCollisionStay(Collision collision)
    {
        if (!isMindjacked && collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage((int)contactDamage);
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}
