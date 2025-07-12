using UnityEngine;

public class RecibirDaño : MonoBehaviour
{
    public ParticleSystem hitParticles;

    void Update()
    {
        // Si presionas la tecla H, simula daño
        if (Input.GetKeyDown(KeyCode.H))
        {
            ActivarParticulas();
        }
    }

    void ActivarParticulas()
    {
        if (hitParticles != null)
        {
            hitParticles.transform.position = transform.position;
            hitParticles.Play();
        }
    }
}

