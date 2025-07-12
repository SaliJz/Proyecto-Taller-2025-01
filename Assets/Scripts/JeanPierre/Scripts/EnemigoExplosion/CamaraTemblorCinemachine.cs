using Cinemachine;
using UnityEngine;

public class CamaraTemblorCinemachine : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float duracion = 0.5f;
    public float intensidad = 2f;
    public float frecuencia = 2f;

    private float tiempoTemblorRestante;
    private CinemachineBasicMultiChannelPerlin perlin;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("No se ha asignado la CinemachineVirtualCamera.");
            return;
        }

        // Intenta obtener el componente Perlin, si no existe, lo a�ade autom�ticamente
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin == null)
        {
            // Fuerza la creaci�n de la extensi�n CinemachineNoise con el componente necesario
            var noise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            perlin = noise;
            Debug.Log("Se a�adi� autom�ticamente el componente Perlin a la c�mara.");
        }

        // Aseg�rate de que la amplitud y frecuencia est�n en cero al comenzar
        perlin.m_AmplitudeGain = 0f;
        perlin.m_FrequencyGain = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivarTemblor();
        }

        if (tiempoTemblorRestante > 0)
        {
            tiempoTemblorRestante -= Time.deltaTime;

            if (tiempoTemblorRestante <= 0f)
            {
                perlin.m_AmplitudeGain = 0f;
                perlin.m_FrequencyGain = 0f;
            }
        }
    }

    public void ActivarTemblor()
    {
        if (perlin == null) return;

        perlin.m_AmplitudeGain = intensidad;
        perlin.m_FrequencyGain = frecuencia;
        tiempoTemblorRestante = duracion;
    }
}
