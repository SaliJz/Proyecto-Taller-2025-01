using System.Collections;
using UnityEngine;

public class PlataformaVertical : MonoBehaviour
{
    TutorialManager0 manager;
    public float altura = 3f;
    public float tiempoSubida = 2f;
    public float tiempoBajada = 2f;

    private Vector3 posicionInicial;
    public bool isActive = false;
    private bool subiendo = true;
    private bool enMovimiento = false;

    void Start()
    {
        manager = TutorialManager0.Instance;
        posicionInicial = transform.position;

        // Suscribirse al evento
        manager.OnPlayerArrivedToCenter += ActivarPlataforma;
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.OnPlayerArrivedToCenter -= ActivarPlataforma;
    }

    void ActivarPlataforma()
    {
        if (!enMovimiento)
            StartCoroutine(MoverPlataforma());
    }

    private IEnumerator MoverPlataforma()
    {
        isActive = true;
        enMovimiento = true;

        Vector3 destino = subiendo
            ? posicionInicial + Vector3.up * altura
            : posicionInicial - Vector3.up * altura;

        float duracion = subiendo ? tiempoSubida : tiempoBajada;
        float t = 0f;
        Vector3 origen = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            transform.position = Vector3.Lerp(origen, destino, t);
            yield return null;
        }

        // Cambia de dirección
        subiendo = !subiendo;
        isActive = false;
        enMovimiento = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.StartCoroutine(manager.TemporarilyDisablePlayerScripts(6));
            manager.StartCoroutine(manager.MovePlayerToPlatformCenter(manager.targetPosition1));
        }
    }
}
