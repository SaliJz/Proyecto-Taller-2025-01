using System.Collections;
using UnityEngine;

public class PlataformaVertical : MonoBehaviour
{
    TutorialManager0 manager;
    public float altura = 3f;
    public float tiempoSubida = 2f;
    public float tiempoBajada = 2f;

    private Vector3 posicionInicial;
    private Vector3 origen;
    private Vector3 destino;
    private float duracion;
    private float t = 0f;

    private bool subiendo = true;
    private bool enMovimiento = false;

    void Start()
    {
        manager = TutorialManager0.Instance;
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (enMovimiento)
        {
            t += Time.deltaTime / duracion;
            transform.position = Vector3.Lerp(origen, destino, t);

            if (t >= 1f)
            {
                StopAllCoroutines();
                transform.position = destino;
                subiendo = !subiendo;
                enMovimiento = false;
                StartCoroutine(EsperaParaActivarPlataforma());

            }
        }
    }

    IEnumerator EsperaParaActivarPlataforma()
    {
        yield return new WaitForSeconds(3f);
        ActivarPlataforma();
    }

    void ActivarPlataforma()
    {
        if (!enMovimiento)
        {
            enMovimiento = true;
            t = 0f;

            origen = transform.position;
            if (subiendo)
            {
                destino = posicionInicial + Vector3.up * altura;
            }
            else
            {
                destino = posicionInicial; // Siempre regresa al punto de origen
            }

            duracion = subiendo ? tiempoSubida : tiempoBajada;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager.currentDialogueIndex == 4)
        {
            manager.ConfirmAdvance();
            foreach (var collider in GetComponents<Collider>())
            {
                if (collider.isTrigger)
                {
                    Destroy(collider);
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && subiendo /*&& manager.currentDialogueIndex > 4*/)
        {
            ActivarPlataforma();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
                enMovimiento = false;
                subiendo = false; // Cambia a modo bajada
                ActivarPlataforma(); // Inicia el movimiento de regreso
        }
    }
}
