using UnityEngine;

public class ParticleMaterialByEnemyRosa : MonoBehaviour
{
    [Tooltip("Sistema de partículas al que se le cambiará el material")]
    [SerializeField] private ParticleSystem particleSystem;

    [Tooltip("Material que se asignará al sistema de partículas")]
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
