using UnityEngine;

public class RotateRig : MonoBehaviour
{
    [Header("Rotación orbital")]
    public float rotationSpeed = 20f;

    [Header("Cámara")]
    public Transform cameraTransform;

    [Header("Órbita")]
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
            // Posiciona la cámara a una altura fija y a cierta distancia hacia atrás (Z negativa)
            cameraTransform.localPosition = new Vector3(0f, height, -radius);
        }
    }
}
