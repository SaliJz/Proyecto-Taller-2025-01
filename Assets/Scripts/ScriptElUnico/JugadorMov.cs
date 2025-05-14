using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorMov : MonoBehaviour
{
    public float velocidad = 5f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direccion = new Vector3(h, 0, v).normalized;
        Vector3 movimiento = transform.forward * direccion.z + transform.right * direccion.x;
        transform.Translate(movimiento * velocidad * Time.deltaTime, Space.World);
    }
}
