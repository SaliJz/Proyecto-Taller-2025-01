using UnityEngine;

public class IgnitionAreaEffect : BaseAreaEffect
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ApplyIgnition(transform.position);
        }

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
            }
        }

        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyIgnition(damagePerSecond, duration);
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

    protected override void ApplyEffectToTargets()
    {
        ApplyDamageToEnemies();
        ApplyDamageToPlayers();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(1f, 0.4f, 0f, 0.3f);
    }
}