using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionCodeShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private LayerMask enemyLayer;

    [SerializeField]
    private GameObject ignitionAreaEffectPrefab;

    public void Initialize(float radius, float damagePerSecond, float duration, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.enemyLayer = enemyLayer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ApplyIgnition(transform.position);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            ApplyIgnitionArea(transform.position);
        }

        Destroy(gameObject);
    }

    private void ApplyIgnition(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyAITest enemy = col.GetComponent<EnemyAITest>();
                if (enemy != null)
                {
                    enemy.ApplyIgnition(damagePerSecond, duration);
                    // Aqu� puedes agregar un efecto de fuego en el enemigo
                }
            }
        }

        // Aqu� puedes instanciar un efecto visual de explosi�n o quemadura
    }

    private void ApplyIgnitionArea(Vector3 center)
    {
        // Instanciar el efecto visual de ascuas
        if (ignitionAreaEffectPrefab != null)
        {
            GameObject ignitionEffect = Instantiate(ignitionAreaEffectPrefab, center, Quaternion.identity);
            IgnitionAreaEffect areaEffect = ignitionEffect.GetComponent<IgnitionAreaEffect>();
            if (areaEffect != null)
            {
                areaEffect.Initialize(radius, damagePerSecond, duration, enemyLayer);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
