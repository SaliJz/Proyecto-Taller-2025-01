using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAITest : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float life = 100f;
    [SerializeField] private int fragments = 50; // Fragmentos de información que suelta el enemigo
    private float currentSpeed;

    // Referencias a las coroutines activas
    private Coroutine slowCoroutine;
    private Coroutine electroHackCoroutine;
    private Coroutine ignitionCoroutine;

    private void Start()
    {
        currentSpeed = baseSpeed;
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

    public void TakeDamage(float dmg)
    {
        life -= dmg; // Resta el daño a la vida del enemigo
        if (life <= 0)
        {
            Destroy(gameObject); // Destruye el enemigo al morir
            HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragmentos de información
        }
    }

    private void Update()
    {
        // ejemplo simple
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
