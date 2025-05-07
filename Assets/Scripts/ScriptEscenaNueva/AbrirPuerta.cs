using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbrirPuerta : MonoBehaviour
{
    public Animator animator;
    public float tiempoAntesDeCambiarEscena = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("Abrir");
            StartCoroutine(CambiarEscenaConRetraso());
        }
    }

    IEnumerator CambiarEscenaConRetraso()
    {
        yield return new WaitForSeconds(tiempoAntesDeCambiarEscena);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Joaquin");
    }
}
