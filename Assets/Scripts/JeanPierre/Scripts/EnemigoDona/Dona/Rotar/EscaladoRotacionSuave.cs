using UnityEngine;

public class EscaladoRotacionSuave : MonoBehaviour
{
    [Header("Referencia al Toro Procedural")]
    [Tooltip("Arrastra aquí tu componente CreadorColisionToro")]
    public CreadorColisionToro toro;

    [Header("Pulsación del Radio Mayor")]
    [Tooltip("Velocidad a la que sube y baja el radio")]
    public float escalaVelocidad = 1f;
    [Tooltip("Máxima variación del radio mayor")]
    public float escalaAmplitud = 0.5f;

    [Header("Rotación en Eje Y")]
    [Tooltip("Grados por segundo")]
    public float velocidadRotacion = 30f;
    public bool rotacionActiva = true;

    private float radioInicial;

    void Start()
    {
        if (toro == null)
        {
            Debug.LogError("EscaladoRotacionSuave: debes asignar CreadorColisionToro en el Inspector.");
            enabled = false;
            return;
        }
        // Guardamos el valor original para pulsar alrededor de él
        radioInicial = toro.radioMayor;
    }

    void Update()
    {
        // Calcula un factor oscilante entre 0 y escalaAmplitud
        float factor = Mathf.PingPong(Time.time * escalaVelocidad, escalaAmplitud);

        // Aplica solo al radio mayor del toro, **no al transform.localScale**
        toro.ActualizarRadioMayor(radioInicial + factor);

        // Rotación pura del GameObject, sin modificar escala
        if (rotacionActiva)
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
    }
}





//using UnityEngine;

//public class EscaladoRotacionSuave : MonoBehaviour
//{
//    [Header("Configuración de Escalado (Radio Mayor)")]
//    [Tooltip("Arrastra aquí tu CreadorColisionToro")]
//    public CreadorColisionToro toro;
//    [Tooltip("Velocidad a la que sube y baja el radio")]
//    public float escalaVelocidad = 1f;
//    [Tooltip("Máxima variación del radio mayor")]
//    public float escalaAmplitud = 0.5f;

//    [Header("Configuración de Rotación (Eje Y)")]
//    public float velocidadRotacion = 30f;
//    public bool rotacionActiva = true;

//    private float radioInicial;

//    void Awake()
//    {
//        // Si existe un Rigidbody, hazlo cinemático para permitir MeshCollider cóncavo
//        var rb = GetComponent<Rigidbody>();
//        if (rb != null)
//            rb.isKinematic = true;
//    }

//    void Start()
//    {
//        if (toro == null)
//        {
//            Debug.LogError("EscaladoRotacionSuave: asigna el componente CreadorColisionToro en el Inspector.");
//            enabled = false;
//            return;
//        }
//        radioInicial = toro.radioMayor;
//    }

//    void Update()
//    {
//        float factor = Mathf.PingPong(Time.time * escalaVelocidad, escalaAmplitud);
//        float nuevoRadio = radioInicial + factor;
//        toro.ActualizarRadioMayor(nuevoRadio);

//        if (rotacionActiva)
//            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
//    }
//}






