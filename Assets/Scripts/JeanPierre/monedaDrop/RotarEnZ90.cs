using UnityEngine;

[ExecuteInEditMode]
public class RotarEnZ90 : MonoBehaviour
{
    [Header("Configuraci�n de rotaci�n")]
    [Tooltip("Aplica la rotaci�n al inicio.")]
    public bool aplicarEnInicio = true;

    [Tooltip("Permite rotaci�n continua en Z.")]
    public bool rotacionContinua = false;

    [Tooltip("Velocidad de rotaci�n continua (grados por segundo).")]
    public float velocidadRotacion = 90f;

    void Start()
    {
        if (aplicarEnInicio)
        {
            AplicarRotacion();
        }
    }

    void Update()
    {
        if (rotacionContinua)
        {
            transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);
        }
    }

    /// <summary>
    /// Establece la rotaci�n en Z a 90 grados.
    /// </summary>
    public void AplicarRotacion()
    {
        Vector3 eulerActual = transform.eulerAngles;
        eulerActual.z = 90f;
        transform.eulerAngles = eulerActual;
    }
}
