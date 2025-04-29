using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarWarsScroll : MonoBehaviour
{
    public float speed = 10f; 
    public float angle = 30f;

    public Transform cameraTransform; 
    public float fadeStartDistance = 5f;
    public float fadeEndDistance = 20f;

    private CanvasGroup canvasGroup;
    private Vector3 direction;
    private Vector3 initialPosition;

    private float timer = 0f;          
    public float resetTime = 15f;

    private void Start()
    {
        
        transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        canvasGroup = GetComponent<CanvasGroup>();
        direction = transform.up;
        initialPosition = transform.position;
    }

    private void Update()
    {
       
        transform.position += direction * speed * Time.deltaTime;
        timer += Time.deltaTime;

        if (canvasGroup != null && cameraTransform != null)
        {
            float distance = Vector3.Distance(transform.position, cameraTransform.position);
            float alpha = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, distance);
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        if (timer >= resetTime)
        {
            transform.position = initialPosition;
            timer = 0f;
        }
    }

}
