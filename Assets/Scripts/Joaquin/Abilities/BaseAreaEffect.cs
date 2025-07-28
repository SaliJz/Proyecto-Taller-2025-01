using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAreaEffect : MonoBehaviour
{
    protected float maxTargets;
    protected float slowMultiplier;
    protected float radius;
    protected float damagePerSecond;
    protected float duration;
    protected LayerMask targetLayer;

    private HashSet<EnemyAbilityReceiver> affectedEnemies = new HashSet<EnemyAbilityReceiver>();
    private HashSet<PlayerHealth> affectedPlayers = new HashSet<PlayerHealth>();
    private float tickRate = 1.0f;

    public virtual void Initialize(float maxTargets, float slowMultiplier, float radius, float damagePerSecond, float duration, LayerMask enemyLayer)
    {
        this.maxTargets = maxTargets;
        this.slowMultiplier = slowMultiplier;
        this.radius = radius;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.targetLayer = enemyLayer;

        transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

        StartCoroutine(EffectLifecycle());
    }

    private IEnumerator EffectLifecycle()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            ApplyEffect(transform.position);
            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
        Destroy(gameObject);
    }

    private void ApplyEffect(Vector3 center)
    {
        affectedEnemies.Clear();
        affectedPlayers.Clear();

        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);

        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                affectedEnemies.Add(col.GetComponent<EnemyAbilityReceiver>());
            }
            else if (col.CompareTag("Player"))
            {
                affectedPlayers.Add(col.GetComponent<PlayerHealth>());
            }
        }

        ApplyEffectToTargets();
    }

    protected abstract void ApplyEffectToTargets();

    protected void ApplyDamageToEnemies()
    {
        foreach (var enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond);
            }
        }
    }

    protected void ApplyDamageToPlayers()
    {
        foreach (var player in affectedPlayers)
        {
            if (player != null)
            {
                player.TakeDamage((int)damagePerSecond, transform.position);
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (radius > 0)
        {
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawSphere(transform.position, radius);
        }
    }

    protected virtual Color GetGizmoColor()
    {
        return new Color(1f, 1f, 1f, 0.3f);
    }
}
