using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStickToPlatform : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Jugador ha entrado en la plataforma. Emparentando.");
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Jugador ha salido de la plataforma. Desemparentando.");
            collision.transform.SetParent(null);
        }
    }
}
