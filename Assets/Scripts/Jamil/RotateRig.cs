using UnityEngine;

public class RotateRig : MonoBehaviour
{
    [Header("Rotaci�n orbital")]
    public float rotationSpeed = 20f;

    [Header("C�mara")]
    public Transform cameraTransform;

    [Header("�rbita")]
    public float radius = 5f;
    public float height = 2f;

    public bool isActive = false;

    void Start()
    {

        ActualizarPosicionCamara();
    }

    void Update()
    {
        if (isActive)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void OnValidate()
    {
        ActualizarPosicionCamara();
    }

    void ActualizarPosicionCamara()
    {
        if (cameraTransform != null)
        {
            // Posiciona la c�mara a una altura fija y a cierta distancia hacia atr�s (Z negativa)
            cameraTransform.localPosition = new Vector3(0f, height, -radius);
        }
    }
}
