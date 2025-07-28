using UnityEngine;

public class ElectroHackShot : MonoBehaviour
{
    private float radius;
    private float maxTargets;
    private float damagePerSecond;
    private float duration;
    private float slowMultiplier;
    private bool hasExploded = false;
    private LayerMask targetLayer;

    [SerializeField] private GameObject electroAreaEffectPrefab;
    [SerializeField] private ParticleSystem impactEffect;

    public void Initialize(float radius, float maxTargets, float damagePerSecond, float duration, float slowMultiplier, LayerMask targetLayer)
    {
        this.radius = radius;
        this.maxTargets = maxTargets;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.slowMultiplier = slowMultiplier;
        this.targetLayer = targetLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Enemy"))
        {
            hasExploded = true;
            PlayEffect();
            ApplyElectroHack(transform.position);
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Ground"))
        {
            hasExploded = true;
            ApplyElectroArea(transform.position);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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

    private void ApplyElectroHack(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);
        int affectedCount = 0;

        foreach (Collider col in hits)
        {
            if (affectedCount >= maxTargets) break;

            if (col.CompareTag("Enemy"))
            {
                EnemyAbilityReceiver enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null)
                {
                    enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
                    affectedCount++;
                }
            }
        }
    }

    private void ApplyElectroArea(Vector3 center)
    {
        if (electroAreaEffectPrefab != null)
        {
            GameObject electroArea = Instantiate(electroAreaEffectPrefab, center, Quaternion.identity);
            ElectroAreaEffect areaEffect = electroArea.GetComponent<ElectroAreaEffect>();
            if (areaEffect != null)
            {
                areaEffect.Initialize(maxTargets, slowMultiplier, radius, damagePerSecond, duration, targetLayer);
            }
        }
        else
        {
            Debug.LogError("Electro Area Effect Prefab no está asignado en ElectroHackShot.");
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
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
