using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSafeZone : MonoBehaviour
{
    private PlataformaVertical plataformaVertical;

    private void Start()
    {
        plataformaVertical = GetComponentInParent<PlataformaVertical>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            plataformaVertical.EnMovimiento = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            plataformaVertical.EnMovimiento = true;
        }
    }
}
