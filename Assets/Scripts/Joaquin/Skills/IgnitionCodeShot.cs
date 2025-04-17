using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionCodeShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private int maxEnemies;
    private LayerMask enemyLayer;

    public void Initialize(float radius, float damagePerSecond, float duration, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.enemyLayer = enemyLayer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Ground"))
        {
            ApplyIgnitionArea(transform.position);
        }

        Destroy(gameObject);
    }

    private void ApplyIgnitionArea(Vector3 center)
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
                    // Aquí puedes agregar un efecto de fuego en el enemigo
                }
            }
        }

        // Aquí puedes instanciar un efecto visual de explosión o quemadura
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
