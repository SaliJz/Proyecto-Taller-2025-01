using UnityEngine;

public class ElectroAreaEffect : BaseAreaEffect
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ApplyElectroHack(transform.position);
        }

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
            }
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
            }
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
            }
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
            }
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
            }
        }

        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
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

    protected override void ApplyEffectToTargets()
    {
        ApplyDamageToEnemies();
        ApplyDamageToPlayers();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(0.2f, 0.8f, 1f, 0.3f);
    }
}