using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletDamage;

    public void Initialize(float damage)
    {
        bulletDamage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        VidaEnemigoGeneral enemigo = collision.gameObject.GetComponent<VidaEnemigoGeneral>();

        // Verifica si la bala ha colisionado con un objeto
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Daño al enemigo
            if (enemigo != null)
            {
                enemigo.RecibirDanio(bulletDamage);
            }

            Debug.Log("Hit enemy!");
            Destroy(gameObject); // Destruye la bala al impactar
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Hit wall!");
            Destroy(gameObject); // Destruye la bala al impactar
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject); // Destruye la bala al impactar
        }
    }
}
