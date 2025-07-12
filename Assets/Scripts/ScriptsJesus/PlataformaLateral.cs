using UnityEngine;

public class PlataformaLateral : MonoBehaviour
{
    public float velocidad = 2f;
    public float distancia = 3f;

    private Vector3 posicionInicial;
    private bool moviendoDerecha = true;

    void Start()
    {
        posicionInicial = transform.position;
    }


    void Update()
    {
        if (moviendoDerecha)
        {
            transform.position += Vector3.right * velocidad * Time.deltaTime;
            if (transform.position.x >= posicionInicial.x + distancia)
            {
                moviendoDerecha = false;
            }
        }
        else
        {
            transform.position += Vector3.left * velocidad * Time.deltaTime;
            if (transform.position.x <= posicionInicial.x - distancia)
            {
                moviendoDerecha = true;
            }
        }
    }
}
