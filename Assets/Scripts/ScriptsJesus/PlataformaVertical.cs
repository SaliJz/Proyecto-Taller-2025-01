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
    private bool playerEnZonaSegura = false;

    public bool EnMovimiento
    {
        get { return enMovimiento; }
        set { enMovimiento = value; }
    }

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
            if (!subiendo && playerEnZonaSegura)
            {
                Debug.Log("Plataforma no bajará: jugador en zona segura");
                return;
            }

            // Si la plataforma va a bajar, revisa si hay un jugador debajo
            if (!subiendo)
            {
                Vector3 boxSize = new Vector3(transform.localScale.x * 0.5f, 0.1f, transform.localScale.z * 0.5f);
                Vector3 boxOrigin = transform.position;
                Vector3 direction = Vector3.down;
                float distanciaDeteccion = altura; // Altura total de bajada

                if (Physics.BoxCast(boxOrigin, boxSize, direction, out RaycastHit hit, Quaternion.identity, distanciaDeteccion))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("Jugador detectado debajo. Plataforma no bajará.");
                        return;
                    }
                }
            }

            // Proceder con el movimiento
            enMovimiento = true;
            t = 0f;

            origen = transform.position;
            destino = subiendo ? posicionInicial + Vector3.up * altura : posicionInicial;
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

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 boxSize = new Vector3(transform.localScale.x * 0.5f, 0.1f, transform.localScale.z * 0.5f);
        Vector3 direction = Vector3.down;
        float distancia = altura;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(direction * distancia, boxSize * 2);
    }
}
