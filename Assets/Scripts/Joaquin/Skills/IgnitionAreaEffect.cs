using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionAreaEffect : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private LayerMask enemyLayer;

    public void Initialize(float radius, float damagePerSecond, float duration, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.enemyLayer = enemyLayer;

        StartCoroutine(EffectDuration());
    }

    private IEnumerator EffectDuration()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            ApplyDamageToEnemies();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Destruir el efecto después de la duración
    }

    private void ApplyDamageToEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyAITest enemy = col.GetComponent<EnemyAITest>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
