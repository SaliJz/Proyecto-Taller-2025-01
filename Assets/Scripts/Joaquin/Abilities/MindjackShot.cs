using UnityEngine;
using static BalaPlayer;

public class MindjackShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;

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
        EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();

        if (other.CompareTag("Enemy"))
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }
        
        if (other.CompareTag("Ground"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Columns"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Roof"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }
        
        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
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

    private void PlayEffect()
    {
        if (impactEffect != null)
        {
            GameObject impactInstance = Instantiate(impactEffect, transform.position, Quaternion.identity).gameObject;
            Destroy(impactInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Particle System no est� asignado en MindjackShot.");
        }
        if (projectilePrefab != null)
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Destroy(projectileInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Projectile Prefab no est� asignado en MindjackShot.");
        }
    }
}