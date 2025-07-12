using UnityEngine;

public class ActivadorPuerta : MonoBehaviour
{
    public PuertaDoble puerta;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puerta.ActivarPuerta();
        }
    }
}
