using System;
using System.Reflection;
using UnityEngine;

public class ParticleMaterialByEnemyType : MonoBehaviour
{
    [Tooltip("Arrastra aqu� el componente VidaEnemigoGeneral que controla este enemigo")]
    public VidaEnemigoGeneral vidaEnemigoGeneral;

    [Tooltip("Sistema de part�culas al que se le cambiar� el material")]
    public ParticleSystem particleSystem;

    [Header("Materiales por tipo de enemigo")]
    public Material materialAmetralladora;
    public Material materialPistola;
    public Material materialEscopeta;

    private ParticleSystemRenderer psRenderer;

    void Awake()
    {
        // Buscar autom�ticamente VidaEnemigoGeneral si no se asign�
        if (vidaEnemigoGeneral == null)
        {
            vidaEnemigoGeneral = GetComponent<VidaEnemigoGeneral>()
                                   ?? GetComponentInParent<VidaEnemigoGeneral>();
        }

        // Obtener el renderer del sistema de part�culas si se asign�
        if (particleSystem != null)
        {
            psRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        }
    }

    void Start()
    {
        if (vidaEnemigoGeneral == null || psRenderer == null)
        {
            Debug.LogError("[ParticleMaterialByEnemyType] Falta referencia a VidaEnemigoGeneral o ParticleSystemRenderer.");
            return;
        }

        // Usamos reflexi�n para acceder al campo privado 'tipo'
        FieldInfo tipoField = typeof(VidaEnemigoGeneral)
                              .GetField("tipo", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tipoField == null)
        {
            Debug.LogError("[ParticleMaterialByEnemyType] No se encontr� el campo 'tipo' en VidaEnemigoGeneral.");
            return;
        }

        // Obtenemos el valor del enum
        var tipoValue = (Enum)tipoField.GetValue(vidaEnemigoGeneral);
        string tipoName = tipoValue.ToString();

        // Seleccionamos el material correspondiente
        Material selectedMat = tipoName switch
        {
            "Ametralladora" => materialAmetralladora,
            "Pistola" => materialPistola,
            "Escopeta" => materialEscopeta,
            _ => null
        };

        if (selectedMat != null)
        {
            psRenderer.material = selectedMat;
        }
        else
        {
            Debug.LogWarning($"[ParticleMaterialByEnemyType] Tipo '{tipoName}' no tiene material asignado.");
        }
    }
}
