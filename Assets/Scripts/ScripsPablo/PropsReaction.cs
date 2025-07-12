using System.Collections.Generic;
using UnityEngine;

public class PropsReaction : MonoBehaviour
{
    [Header("Parámetros de vida")]
    [SerializeField] private float vida = 3;
    float TimerDead = 0;

    [Header("Rigidbodies hijos")]
    private List<Rigidbody> rigidbodiesHijos = new List<Rigidbody>();

    void Start()
    {
        Rigidbody[] encontrados = GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in encontrados)
        {
            if (rb != GetComponent<Rigidbody>())
            {
                rb.isKinematic = true;
                rigidbodiesHijos.Add(rb);
            }
        }
    }

    void Update()
    {
        if (vida <= 0)
        {
            ActivarRigidbodies();
            TimerDead += Time.deltaTime;
            if (TimerDead >= 3)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ActivarRigidbodies()
    {
        foreach (Rigidbody rb in rigidbodiesHijos)
        {
            rb.isKinematic = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GunBullet"))
        {
            vida--;
        }
    }
}
