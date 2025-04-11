using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookVisual : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private float rotationSpeed = 10f;

    private void Update()
    {
        Vector3 lookDirection = orientation.forward;
        lookDirection.y = 0f; // evita inclinación vertical

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
