using UnityEngine;

[ExecuteInEditMode]
public class RotarEnZ90 : MonoBehaviour
{
    [Header("Configuración de rotación")]
    [Tooltip("Aplica la rotación al inicio.")]
    public bool aplicarEnInicio = true;

    [Tooltip("Permite rotación continua en Z.")]
    public bool rotacionContinua = false;

    [Tooltip("Velocidad de rotación continua (grados por segundo).")]
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
    /// Establece la rotación en Z a 90 grados.
    /// </summary>
    public void AplicarRotacion()
    {
        Vector3 eulerActual = transform.eulerAngles;
        eulerActual.z = 90f;
        transform.eulerAngles = eulerActual;
    }
}
