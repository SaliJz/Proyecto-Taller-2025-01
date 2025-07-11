using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float verticalClamp = 90f;
    [SerializeField] private Transform orientation;

    private float rotationX;
    private float rotationY;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // Captura rotación inicial desde la escena
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        rotationX = eulerAngles.x;
        rotationY = eulerAngles.y;
    }

    private void LateUpdate()
    {
        float currentSensitivity = SettingsService.Sensitivity * 100f;
        sensitivity = currentSensitivity;

        float mouseX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * sensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;
        rotationX = Mathf.Clamp(rotationX, -verticalClamp, verticalClamp);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
}
