using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarWarsScroll : MonoBehaviour
{
    [SerializeField] private float speed = 10f; 
    [SerializeField] private float angle = 30f;

    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Vector3 finalPosition;

    private Vector3 direction;
    private Vector3 scrollDirection;
    private float scrollLength;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        direction = transform.up;
        initialPosition = transform.position;

        scrollDirection = (finalPosition - initialPosition).normalized;
        scrollLength = Vector3.Distance(finalPosition, initialPosition);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        float movedDistance = Vector3.Dot(transform.position - initialPosition, scrollDirection);

        if (movedDistance >= scrollLength)
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        // Resetea a la posición inicial.
        transform.position = initialPosition;
    }
}
