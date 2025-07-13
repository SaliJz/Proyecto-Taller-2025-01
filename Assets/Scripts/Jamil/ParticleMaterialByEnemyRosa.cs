using System;
using System.Reflection;
using UnityEngine;

public class ParticleMaterialByEnemyRosa: MonoBehaviour
{
    [Tooltip("Arrastra aquí el componente VidaEnemigoGeneral que controla este enemigo")]
    public EnemigoRosa enemigoRosa;

    [Tooltip("Sistema de partículas al que se le cambiará el material")]
    public ParticleSystem particleSystem;

    [Header("Materiales por tipo de enemigo")]
    public Material materialAmetralladora;
    public Material materialPistola;
    public Material materialEscopeta;

    private ParticleSystemRenderer psRenderer;

    void Awake()
    {
        
        if (particleSystem != null)
        {
            psRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        }
    }

    void Start()
    {
        // Usamos reflexión para acceder al campo privado 'tipo'
        FieldInfo tipoField = typeof(EnemigoRosa).GetField("tipo", BindingFlags.NonPublic | BindingFlags.Instance);
        // Obtenemos el valor del enum
        var tipoValue = (Enum)tipoField.GetValue(enemigoRosa);
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
