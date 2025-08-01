using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAbilityReceiver : MonoBehaviour
{
    public enum EnemyState { Normal, Mindjacked }
    public EnemyState CurrentState { get; private set; } = EnemyState.Normal;
    public Transform CurrentTarget { get; private set; }

    public event Action<EnemyState> OnStateChanged;

    [SerializeField] private bool isImmuneToMindControl = false;
    //[SerializeField] private float mindjackFindTargetRadius = 10f;
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

    private GameObject activeGlitchParticle;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        CurrentTarget = playerTransform;
        currentSpeed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        SendMessage("ApplyAbilityDamage", dmg);
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
            var particleSystem = effectToDestroy.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.loop = false;
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            Destroy(effectToDestroy, 0.25f);
        }

        isSlowed = false;
        slowCoroutine = null;
    }

    public void ApplyGlitchTime(float slowMultiplier, float duration, GameObject particlePrefab)
    {
        Debug.Log(gameObject + " applying glitch time with multiplier: " + slowMultiplier + " and duration: " + duration);

        if (activeGlitchParticle != null)
        {
            Destroy(activeGlitchParticle);
            activeGlitchParticle = null;
        }

        if (particlePrefab != null)
        {
            activeGlitchParticle = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
        }

        ApplySlow(slowMultiplier, duration, activeGlitchParticle);
    }

    public GameObject GetActiveGlitchParticle()
    {
        return activeGlitchParticle;
    }

    public void ApplyElectroHack(float damagePerSecond, float duration, float slowMultiplier)
    {
        Debug.Log(gameObject + " applying electro hack with damage: " + damagePerSecond + ", duration: " + duration + ", slow multiplier: " + slowMultiplier);

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
            SendMessage("ApplyAbilityDamage", damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isElectroHacked = false;
        electroHackCoroutine = null;
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        Debug.Log(gameObject + " applying ignition with damage: " + damagePerSecond + ", duration: " + duration);

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
            SendMessage("ApplyAbilityDamage", damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isIgnited = false;
        ignitionCoroutine = null;
    }

    public void ApplyMindjack(float damagePerSecond, float duration)
    {
        Debug.Log(gameObject + " applying mindjack with damage: " + damagePerSecond + ", duration: " + duration);

        if (isMindjacked) return;
        if (mindjackCoroutine != null) StopCoroutine(mindjackCoroutine);
        mindjackCoroutine = StartCoroutine(MindjackRoutine(damagePerSecond, duration));
    }

    private IEnumerator MindjackRoutine(float damagePerSecond, float duration)
    {
        isMindjacked = true;
        if (!isImmuneToMindControl)
        {
            CurrentState = EnemyState.Mindjacked;
            OnStateChanged?.Invoke(CurrentState);
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            //SendMessage("ApplyAbilityDamage", damagePerSecond);
            yield return new WaitForSeconds(1.0f);
            elapsedTime += 1.0f;
        }

        isMindjacked = false;
        if (!isImmuneToMindControl)
        {
            CurrentState = EnemyState.Normal;
            OnStateChanged?.Invoke(CurrentState);
        }
        mindjackCoroutine = null;
    }

    public void StopAllAbilityEffects()
    {
        StopAllCoroutines();

        isMindjacked = false;
        isIgnited = false;
        isElectroHacked = false;
        isSlowed = false;
        CurrentState = EnemyState.Normal;
        currentSpeed = baseSpeed;
    }

    //private Transform FindNearestEnemy()
    //{
    //    Collider[] hits = Physics.OverlapSphere(transform.position, mindjackFindTargetRadius, LayerMask.GetMask("Enemy"));
    //    Transform nearestEnemy = null;
    //    float minDistance = float.MaxValue;

    //    foreach (var hit in hits)
    //    {
    //        var rec = hit.GetComponent<EnemyAbilityReceiver>();
    //        if (rec == null || rec.currentState == EnemyState.Mindjacked || rec.isImmuneToMindControl) continue;

    //        if (hit.transform == this.transform) continue;

    //        float distance = Vector3.Distance(transform.position, hit.transform.position);
    //        if (distance < minDistance)
    //        {
    //            minDistance = distance;
    //            nearestEnemy = hit.transform;
    //        }
    //    }
    //    return nearestEnemy;
    //}
}