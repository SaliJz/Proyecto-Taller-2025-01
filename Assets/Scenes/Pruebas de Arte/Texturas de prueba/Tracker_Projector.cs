using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker_Projector : MonoBehaviour
{
    public Transform followTarget; // El enemigo u objeto que se mueve
    public Vector3 offset;

    void LateUpdate()
    {
        if (followTarget != null)
        {
            // Posiciona la luz en el mismo lugar que el objetivo + offset
            transform.position = followTarget.position + offset;

            // Siempre mira hacia abajo
            transform.rotation = Quaternion.LookRotation(Vector3.down);
        }
    }
}
