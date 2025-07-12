using UnityEngine;

public class ElevadorTutorial : MonoBehaviour
{
    public Transform objetivo; // Punto a mover
    public float velocidadMovimiento = 2f;
    public float tiempoEspera = 5f;

    private Vector3 posicionInicial;
    private bool playerEncima;
    private float tiempoFuera = 0f;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (playerEncima)
        {
            transform.position = Vector3.MoveTowards(transform.position, objetivo.position, velocidadMovimiento * Time.deltaTime);
        }
        else
        {
            tiempoFuera += Time.deltaTime;
            if (tiempoFuera >= tiempoEspera && transform.position != posicionInicial)
            {
                transform.position = Vector3.MoveTowards(transform.position, posicionInicial, velocidadMovimiento * Time.deltaTime);
            }
        }
    }
    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerEncima = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerEncima = false;
        }
    }
}
