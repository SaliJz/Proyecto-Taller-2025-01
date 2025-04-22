using UnityEngine;

public class EscaladoRotacionSuave : MonoBehaviour
{
    [Header("Configuración de Escalado")]
    public float escalaVelocidad = 1f;    // Velocidad del escalado
    public float escalaAmplitud = 0.5f;   // Rango máximo de escalado
    public bool escaladoActivo = true;    // Activar/desactivar escalado

    [Header("Configuración de Rotación (Eje Y)")]
    public float velocidadRotacion = 30f; // Velocidad de rotación en grados/segundo
    public bool rotacionActiva = true;    // Activar/desactivar rotación

    private Vector3 escalaInicial;
    private Vector3 escalaObjetivo;

    void Start()
    {
        // Guardar la escala inicial del objeto
        escalaInicial = transform.localScale;
    }

    void Update()
    {
        // Escalado pulsante en X y Z
        if (escaladoActivo)
        {
            float factorEscala = Mathf.Sin(Time.time * escalaVelocidad) * escalaAmplitud;
            transform.localScale = new Vector3(
                escalaInicial.x + factorEscala,
                escalaInicial.y,
                escalaInicial.z + factorEscala
            );
        }

        // Rotación continua en el eje Y
        if (rotacionActiva)
        {
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
        }
    }

    // Métodos para controlar los efectos desde otros scripts
    public void ToggleEscalado(bool estado)
    {
        escaladoActivo = estado;
        if (!estado) transform.localScale = escalaInicial;
    }

    public void ToggleRotacion(bool estado)
    {
        rotacionActiva = estado;
    }
}