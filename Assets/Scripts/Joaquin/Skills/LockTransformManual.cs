using UnityEngine;

public class LockTransformManual : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
