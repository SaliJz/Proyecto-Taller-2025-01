using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Efectos_Tester : MonoBehaviour
{
    public GameObject VFX;

    void Update()
    {
        // Si presionas la tecla H, simula daño
        if (Input.GetKeyDown(KeyCode.J))
        {
            ActivarParticulas();
        }
    }

    void ActivarParticulas()
    {
        if (VFX != null)
        {
            GameObject obj = Instantiate(VFX);
            obj.transform.position = transform.position;
            
        }
    }
}

