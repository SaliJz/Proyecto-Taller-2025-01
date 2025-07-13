using UnityEngine;

public class IgnitionCodeShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private bool hasExploded = false;
    private LayerMask targetLayer;

    [SerializeField] private GameObject ignitionAreaEffectPrefab;
    [SerializeField] private ParticleSystem impactEffect;

    public void Initialize(float radius, float damagePerSecond, float duration, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.targetLayer = enemyLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Enemy"))
        {
            hasExploded = true;
            PlayEffect();
            ApplyIgnition(transform.position);
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Ground"))
        {
            hasExploded = true;
            ApplyIgnitionArea(transform.position);
            Destroy(gameObject);
        }


        if (other.CompareTag("Wall") || other.CompareTag("Columns") || other.CompareTag("Roof"))
        {
            hasExploded = true;
            PlayEffect();
            Destroy(gameObject);
        }
    }

    private void ApplyIgnition(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyAbilityReceiver enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null)
                {
                    enemy.ApplyIgnition(damagePerSecond, duration);
                }
            }
        }
    }

    private void ApplyIgnitionArea(Vector3 center)
    {
        if (ignitionAreaEffectPrefab != null)
        {
            GameObject ignitionEffect = Instantiate(ignitionAreaEffectPrefab, center, Quaternion.identity);
            IgnitionAreaEffect areaEffect = ignitionEffect.GetComponent<IgnitionAreaEffect>();
            if (areaEffect != null)
            {
                areaEffect.Initialize(radius, damagePerSecond, duration, targetLayer);
            }
        }
        else
        {
            Debug.LogWarning("Ignition Area Effect Prefab no está asignado en IgnitionCodeShot.");
        }
    }

    private void PlayEffect()
    {
        if (impactEffect != null)
        {
            GameObject impactInstance = Instantiate(impactEffect, transform.position, Quaternion.identity).gameObject;
            Destroy(impactInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Particle System no está asignado en MindjackShot.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
