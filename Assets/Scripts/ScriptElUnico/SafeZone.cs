using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    public bool playerInside = false;
    public int enemiesInside = 0;

    public delegate void PlayerEnterExit(bool entered);
    public event PlayerEnterExit OnPlayerStateChange;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Jugador ENTR� en la zona segura");
            OnPlayerStateChange?.Invoke(true);
        }

        if (other.CompareTag("Enemy"))
        {
            enemiesInside++;
            Debug.Log($"Enemigo entr�. Total enemigos: {enemiesInside}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Jugador SALI� de la zona segura");
            OnPlayerStateChange?.Invoke(false);
        }


        if (other.CompareTag("Enemy"))
        {
            enemiesInside--;
            Debug.Log($"Enemigo sali�. Total enemigos: {enemiesInside}");
        }
    }
}
