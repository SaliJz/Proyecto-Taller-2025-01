using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker_Projector : MonoBehaviour
{
    public Light spotLight;            // Tu luz con cookie
    public Transform eyeTransform;     // Transform del "ojo" del personaje
    public float heightOffset = 0.1f;  // Qué tan lejos de la superficie

    void LateUpdate()
    {
        // Hacer que la luz siga al ojo y siempre mire hacia abajo
        spotLight.transform.position = eyeTransform.position;
        spotLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Siempre mira hacia abajo
    }
}
