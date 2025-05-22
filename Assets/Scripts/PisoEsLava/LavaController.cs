using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LavaController : MonoBehaviour
{
    public float ascentSpeed = 0.5f; 
    public float damagePerSecond = 5f;
    private float timer = 0f;

    private void Update()
    {
        transform.position += Vector3.up * ascentSpeed * Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
            SaludJugador ph = other.GetComponent<SaludJugador>();
            if (ph != null)
            {
                ph.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }




}
