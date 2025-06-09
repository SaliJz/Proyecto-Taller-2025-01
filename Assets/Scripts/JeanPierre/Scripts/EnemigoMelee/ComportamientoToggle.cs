using UnityEngine;

public class ComportamientoToggle : MonoBehaviour
{
    [Header("Componentes de comportamiento")]
    public MonoBehaviour comportamientoA;
    public MonoBehaviour comportamientoB;

    [Header("Activar componentes")]
    public bool activarA;
    public bool activarB;

    void Update()
    {
        // Si activarA es true, activa A y desactiva B
        if (activarA)
        {
            if (comportamientoA != null) comportamientoA.enabled = true;
            if (comportamientoB != null) comportamientoB.enabled = false;
        }

        // Si activarB es true, activa B y desactiva A
        if (activarB)
        {
            if (comportamientoB != null) comportamientoB.enabled = true;
            if (comportamientoA != null) comportamientoA.enabled = false;
        }
    }
}