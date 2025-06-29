using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchTimeArea : MonoBehaviour
{
    private float radius;
    private float duration;
    private float slowMultiplier;
    private LayerMask enemyLayer;

    [SerializeField] private GameObject glitchParticlePrefab;
    [SerializeField] private float expandSpeed = 1f;

    private HashSet<EnemyAbilityReceiver> affectedEnemies = new HashSet<EnemyAbilityReceiver>();

    public void Initialize(float radius, float duration, float slowMultiplier, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.duration = duration;
        this.slowMultiplier = slowMultiplier;
        this.enemyLayer = enemyLayer;

        StartCoroutine(EffectLifecycle());
    }

    private IEnumerator EffectLifecycle()
    {
        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.one;
        Vector3 targetScale = new Vector3(radius * 2, radius * 2, radius * 2);

        float expandDuration = Mathf.Min(duration, expandSpeed);
        float expandTime = 0f;

        while (expandTime < expandDuration)
        {
            float progress = expandTime / expandDuration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            ApplyEffect(transform.position, radius);
            expandTime += Time.deltaTime;
            yield return null;
        }

        while (elapsedTime < duration)
        {
            ApplyEffect(transform.position, radius);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        Destroy(gameObject);
    }

    private void ApplyEffect(Vector3 center, float currentRadius)
    {
        Collider[] hits = Physics.OverlapSphere(center, currentRadius, enemyLayer);

        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                var enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null && !affectedEnemies.Contains(enemy))
                {
                    affectedEnemies.Add(enemy);
                    enemy.ApplyGlitchTime(slowMultiplier, duration, glitchParticlePrefab);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (radius > 0)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, transform.localScale.x / 2);
        }
    }
}
