using UnityEngine;

public class ParticleMaterialByEnemyRosa : MonoBehaviour
{
    [Tooltip("Sistema de part�culas al que se le cambiar� el material")]
    [SerializeField] private ParticleSystem particleSystem;

    [Tooltip("Material que se asignar� al sistema de part�culas")]
    [SerializeField] private Material materialAsignado;

    private void Awake()
    {
        if (particleSystem != null && materialAsignado != null)
        {
            var psRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.material = materialAsignado;
            }
        }
    }
}
