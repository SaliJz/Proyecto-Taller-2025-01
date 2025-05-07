using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AbrirPuertayEscena : MonoBehaviour
{

    public Animator animator;
    public float tiempoAntesDeCambiarEscena = 1.5f;
    public string nombreEscenaDestino;
    public float distanciaDeActivacion = 5f;

    private bool puertaAbierta = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !puertaAbierta) 
        {
            float distancia = Vector3.Distance(transform.position, other.transform.position);

            if (distancia <= distanciaDeActivacion)
            {
                animator.SetTrigger("Abrir");
                puertaAbierta = true;
                GetComponent<Collider>().enabled = false;
                StartCoroutine(CambiarEscenaConRetraso());
            }
        }
    }

     IEnumerator CambiarEscenaConRetraso()
    {
        yield return new WaitForSeconds(tiempoAntesDeCambiarEscena);
        SceneManager.LoadScene(nombreEscenaDestino);
    }
}
