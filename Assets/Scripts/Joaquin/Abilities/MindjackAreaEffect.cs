using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindjackAreaEffect : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;

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
            }
        }

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
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
}