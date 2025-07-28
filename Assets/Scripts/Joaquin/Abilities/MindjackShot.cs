using UnityEngine;

public class MindjackShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;
    private bool hasExploded = false;

    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private GameObject projectilePrefab;

    public void Initialize(float radius, float dps, float duration)
    {
        this.radius = radius;
        this.damagePerSecond = dps;
        this.duration = duration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();

        string[] onlyExplodesTags = { "Ground", "Wall", "Columns", "Roof" };
        if (System.Array.Exists(onlyExplodesTags, tag => other.CompareTag(tag)))
        {
            Explode();
            return;
        }

        if (other.CompareTag("Enemy") && enemy != null)
        {
            Explode(enemy);
            return;
        }

        if (BuscarComponenteEnPadres<Fase1Vida>(other.transform) != null && enemy != null)
        {
            Explode(enemy);
            return;
        }
        if (BuscarComponenteEnPadres<Fase2Vida>(other.transform) != null && enemy != null)
        {
            Explode(enemy);
            return;
        }
        if (BuscarComponenteEnPadres<Fase3Vida>(other.transform) != null && enemy != null)
        {
            Explode(enemy);
            return;
        }

        if (other.GetComponent<EnemigoPistolaTutorial>() != null && enemy != null)
        {
            Explode(enemy);
            return;
        }
        if (other.GetComponent<VidaEnemigoGeneral>() != null && enemy != null)
        {
            Explode(enemy);
            return;
        }
        if (other.GetComponent<EnemigoRosa>() != null && enemy != null)
        {
            Explode(enemy);
            return;
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

    private void Explode(EnemyAbilityReceiver enemy = null)
    {
        hasExploded = true;
        if (enemy != null) enemy.ApplyMindjack(damagePerSecond, duration);
        PlayEffect();
        Destroy(gameObject);
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
        if (projectilePrefab != null)
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            MindjackAreaEffect mindjackAreaEffect = projectileInstance.GetComponent<MindjackAreaEffect>();

            if (mindjackAreaEffect != null)
            {
                mindjackAreaEffect.Initialize(radius, damagePerSecond, duration);
            }

            Destroy(projectileInstance, 0.5f);
        }
        else
        {
            Debug.LogWarning("Projectile Prefab no está asignado en MindjackShot.");
        }
    }
}