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

    public bool isActive = false;
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
                transform.position = destino;
                subiendo = !subiendo;
                enMovimiento = false;
                isActive = false;
            }
        }
    }

    void ActivarPlataforma()
    {
        if (!enMovimiento)
        {
            isActive = true;
            enMovimiento = true;
            t = 0f;

            origen = transform.position;
            destino = subiendo
                ? posicionInicial + Vector3.up * altura
                : posicionInicial - Vector3.up * altura;

            duracion = subiendo ? tiempoSubida : tiempoBajada;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager.currentDialogueIndex == 4)
        {
            ActivarPlataforma();
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
}
