using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] // Asegúrate de que el objeto tenga un Collider
public class EnemyAbilityReceiver : MonoBehaviour
{
    // Referencia a VidaEnemigoGeneral
    private VidaEnemigoGeneral vida;

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
        if (vida == null)
        {
            Debug.LogWarning("VidaEnemigoGeneral no encontrada en el enemigo.");
        }

        // Inicializar velocidad
        currentSpeed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        vida?.RecibirDanio(dmg);
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
            TakeDamage(tickDamage);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        if (isIgnited) return;
        isIgnited = true;

        if (ignitionCoroutine != null)
        {
            StopCoroutine(ignitionCoroutine);
        }
        ignitionCoroutine = StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
    }

    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
    {
        int ticks = Mathf.FloorToInt(duration);
        for (int i = 0; i < ticks; i++)
        {
            TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
        isIgnited = false;
    }

    public void ApplyMindjack(float damagePerSecond, float duration)
    {
        if (isMindjacked) return;
        isMindjacked = true;

        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>();
        if (mindjackAbility != null)
        {
            mindjackAbility.EnemyMindjacked(true);
        }
        else
        {
            Debug.LogError("No se encontró la habilidad MindjackAbility.");
        }

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

        // Fin del efecto Mindjack
        isMindjacked = false;

        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>();
        if (mindjackAbility != null)
        {
            mindjackAbility.EnemyMindjacked(false);
        }
        else
        {
            Debug.LogError("No se encontró la habilidad MindjackAbility.");
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



//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using UnityEngine;

//[RequireComponent(typeof(Collider))] // Asegúrate de que el objeto tenga un Collider
//public class EnemyAbilityReceiver : MonoBehaviour
//{
//    // Referencia a VidaEnemigoGeneral
//    private VidaEnemigoGeneral vida;

//    [Header("Attack")]
//    [SerializeField] private float contactDamage = 10f;
//    [SerializeField] private float attackCooldown = 1f;
//    [SerializeField] private float attackRange = 20f;

//    [Header("Slow")]
//    [SerializeField] private float baseSpeed = 3f;
//    public float currentSpeed;
//    public float CurrentSpeed => currentSpeed;

//    [Header("Glow")]
//    [SerializeField] private Renderer meshRenderer;
//    [SerializeField] private Material defaultMaterial;
//    [SerializeField] private Material mindjackMaterial;

//    // Estado de las habilidades
//    private bool isMindjacked = false;
//    private bool isIgnited = false;
//    private float lastAttackTime = 0f;

//    // Referencias a las coroutines activas
//    private Coroutine slowCoroutine;
//    private Coroutine electroHackCoroutine;
//    private Coroutine ignitionCoroutine;
//    private Coroutine mindjackCoroutine;

//    private void Start()
//    {
//        currentSpeed = baseSpeed;
//        if (meshRenderer != null && defaultMaterial != null)
//        {
//            meshRenderer.material = defaultMaterial;
//        }
//    }

//    private void Awake()
//    {
//        vida = GetComponent<VidaEnemigoGeneral>();
//        if (vida == null)
//        {
//            Debug.LogWarning("VidaEnemigoGeneral no encontrada en el enemigo.");
//        }
//    }

//    public void TakeDamage(float dmg)
//    {

//        vida?.RecibirDanio(dmg);

//    }

//    public void ApplySlow(float multiplier, float duration)
//    {
//        // Detener la coroutine anterior si está activa
//        if (slowCoroutine != null)
//        {
//            StopCoroutine(slowCoroutine);
//        }

//        StartCoroutine(SlowRoutine(multiplier, duration));
//    }

//    private IEnumerator SlowRoutine(float multiplier, float duration)
//    {
//        currentSpeed = baseSpeed * multiplier;
//        yield return new WaitForSeconds(duration);
//        currentSpeed = baseSpeed;
//    }

//    public void ApplyElectroHack(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
//    {
//        // Detener la coroutine anterior si está activa
//        if (electroHackCoroutine != null)
//        {
//            StopCoroutine(electroHackCoroutine);
//        }

//        StartCoroutine(ElectroHackRoutine(tickDamage, tickInterval, ticks, slowMultiplier));
//    }

//    private IEnumerator ElectroHackRoutine(float tickDamage, float tickInterval, int ticks, float slowMultiplier)
//    {
//        ApplySlow(slowMultiplier, tickInterval * ticks);

//        for (int i = 0; i < ticks; i++)
//        {
//            TakeDamage(tickDamage);
//            yield return new WaitForSeconds(tickInterval);
//        }
//    }

//    public void ApplyIgnition(float damagePerSecond, float duration)
//    {
//        if (isIgnited) return;
//        isIgnited = true;

//        // Detener la coroutine anterior si está activa
//        if (ignitionCoroutine != null)
//        {
//            StopCoroutine(ignitionCoroutine);
//        }

//        StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
//    }

//    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
//    {
//        int ticks = Mathf.FloorToInt(duration);
//        for (int i = 0; i < ticks; i++)
//        {
//            TakeDamage(damagePerSecond);
//            yield return new WaitForSeconds(1f);
//        }

//        isIgnited = false;
//    }

//    public void ApplyMindjack(float damagePerSecond, float duration)
//    {
//        // Detener la coroutine anterior si está activa
//        if (isMindjacked) return;
//        isMindjacked = true;
//        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>(); // Obtener la instancia de la habilidad
//        if (mindjackAbility != null)
//        {
//            mindjackAbility.EnemyMindjacked(true); // Actualizar el estado de la habilidad
//        }
//        else
//        {
//            Debug.LogError("No se encontró la habilidad MindjackAbility.");
//        }

//        // Detener la coroutine anterior si está activa
//        if (mindjackCoroutine != null)
//        {
//            StopCoroutine(mindjackCoroutine);
//        }

//        StartCoroutine(MindjackRoutine(damagePerSecond, duration));

//        if (meshRenderer != null && mindjackMaterial != null)
//        {
//            meshRenderer.material = mindjackMaterial;
//        }
//    }

//    private IEnumerator MindjackRoutine(float damagePerSecond, float duration)
//    {
//        float elapsed = 0f;
//        float damageInterval = 1f;

//        while (elapsed < duration && vida.vida > 0)
//        {
//            GameObject target = FindNearestEnemy();
//            if (target != null)
//            {
//                float distance = Vector3.Distance(transform.position, target.transform.position);

//                // Movimiento hacia el enemigo
//                if (distance > 1.5f) // separación mínima
//                {
//                    transform.LookAt(target.transform);
//                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, currentSpeed * Time.deltaTime);
//                }

//                // Ataque si está cerca
//                if (distance <= 2f && Time.time >= lastAttackTime + attackCooldown)
//                {
//                    EnemyAbilityReceiver other = target.GetComponent<EnemyAbilityReceiver>();
//                    if (other != null && !other.isMindjacked)
//                    {
//                        other.TakeDamage(contactDamage);
//                        lastAttackTime = Time.time;
//                    }
//                }
//            }

//            TakeDamage(damagePerSecond); // daño por segundo

//            yield return new WaitForSeconds(damageInterval);
//            elapsed += damageInterval;
//        }

//        // Fin del efecto Mindjack
//        isMindjacked = false;

//        MindjackAbility mindjackAbility = FindObjectOfType<MindjackAbility>(); // Obtener la instancia de la habilidad
//        if (mindjackAbility != null)
//        {
//            mindjackAbility.EnemyMindjacked(false); // Restablecer el estado de la habilidad
//        }
//        else
//        {
//            Debug.LogError("No se encontró la habilidad MindjackAbility.");
//        }

//        // Restaurar material si es necesario
//        if (meshRenderer != null && defaultMaterial != null)
//        {
//            meshRenderer.material = defaultMaterial;
//        }
//    }

//    private GameObject FindNearestEnemy()
//    {
//        GameObject nearest = null;
//        float shortestDistance = Mathf.Infinity;

//        int enemyLayer = LayerMask.GetMask("Enemy");
//        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

//        foreach (var col in hits)
//        {
//            if (col.gameObject != gameObject)
//            {
//                EnemyAbilityReceiver other = col.GetComponent<EnemyAbilityReceiver>();
//                if (other != null && !other.isMindjacked)
//                {
//                    float distance = Vector3.Distance(transform.position, col.transform.position);
//                    if (distance < shortestDistance)
//                    {
//                        shortestDistance = distance;
//                        nearest = col.gameObject;
//                    }
//                }
//            }
//        }

//        return nearest;
//    }

//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, attackRange);
//    }
//}
