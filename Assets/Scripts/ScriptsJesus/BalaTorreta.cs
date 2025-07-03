using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaTorreta : MonoBehaviour
{

    public float velocidad = 20f;
    public int daño = 20;
    public float tiempoVida = 3f;

    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    
    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            other.GetComponent<JugadorMovimiento>()?.TomarDano(daño);
            Destroy(gameObject);
        }
    }
}
