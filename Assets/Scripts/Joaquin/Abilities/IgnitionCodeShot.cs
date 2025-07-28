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

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }

        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            hasExploded = true;
            PlayEffect();
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
            Destroy(gameObject);
        }
    }

    private T BuscarComponenteEnPadres<T>(Transform hijo) where T : Component
    {
        Transform actual = hijo;
        while (actual != null)
        {
            T componente = actual.GetComponent<T>();
            if (componente != null)
                return componente;
            actual = actual.parent;
        }
        return null;
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
                areaEffect.Initialize(0, 0, radius, damagePerSecond, duration, targetLayer);
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
