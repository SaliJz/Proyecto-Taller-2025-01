using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionAreaEffect : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private LayerMask enemyLayer;

    private HashSet<EnemyAbilityReceiver> affectedEnemies = new HashSet<EnemyAbilityReceiver>();
    private HashSet<PlayerHealth> affectedPlayers = new HashSet<PlayerHealth>();


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
        // Esperar la duración del efecto
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            ApplyIgnition(transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject); // Destruir el efecto después de la duración
    }

    private void ApplyIgnition(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);
        Debug.Log($"[IgnitionArea] Objetos encontrados: {hits.Length}");

        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                var enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null && !affectedEnemies.Contains(enemy))
                {
                    affectedEnemies.Add(enemy);
                }
            }
            else if (col.CompareTag("Player"))
            {
                var player = col.GetComponent<PlayerHealth>();
                if (player != null && !affectedPlayers.Contains(player))
                {
                    affectedPlayers.Add(player);
                }
            }
        }

        // Aplicar daño a los enemigos afectados
        foreach (var enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                Debug.Log($"Aplicando daño por ignición a: {enemy.name}");
                enemy.TakeDamage(damagePerSecond);
            }
        }

        // Aplicar daño al jugador afectado
        foreach (var player in affectedPlayers)
        {
            if (player != null)
            {
                Debug.Log($"Aplicando daño por ignición al jugador");
                player.TakeDamage((int)damagePerSecond);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (radius > 0)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, radius);
        }
        else
        {
            Debug.LogWarning("El radio de ignición es 0 o menor. No se puede dibujar Gizmos.");
        }
    }
}
