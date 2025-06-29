using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchTime : MonoBehaviour
{
    public Transform esfera;
    public Light glitchLight;

    public float duracion = 2f;
    public float escalaMaxima = 5f;
    public float intensidadMaximaLuz = 8f;

    private float tiempo;
    private bool activado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !activado)
        {
            StartCoroutine(ExpandirEsfera());
            activado = true;
        }
    }

    IEnumerator ExpandirEsfera()
    {
        esfera.localScale = Vector3.zero;
        glitchLight.intensity = 0f;
        glitchLight.enabled = true;

        tiempo = 0f;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;

            // Escalar la esfera
            esfera.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * escalaMaxima, t);

            // Aumentar intensidad de la luz
            glitchLight.intensity = Mathf.Lerp(0f, intensidadMaximaLuz, t);

            tiempo += Time.deltaTime;
            yield return null;
        }

        esfera.localScale = Vector3.one * escalaMaxima;
        glitchLight.intensity = intensidadMaximaLuz;
    }
}
