using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayoLaser : MonoBehaviour
{
    public float velocidad = 20f; 
    public int da�o = 10; 

    void Start()
    {
        Destroy(gameObject, 2f);
    }

    
    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Torreta"))
        {
            
            other.GetComponent<Torreta>().TomarDa�o(da�o);
            
            Destroy(gameObject);
        }
        else if (!other.CompareTag("PLAYER") && !other.CompareTag("Laser"))
        {
            Destroy(gameObject); // Se destruye con cualquier otra cosa
        }
    }
}
