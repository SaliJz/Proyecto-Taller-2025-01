using UnityEngine;

public class BalaSimple : MonoBehaviour
{
    public float velocidad = 20f;
    public int daño = 10;

    private Vector3 direccion;
    private Transform jugador;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("PLAYER").transform;

        direccion = (jugador.position - transform.position).normalized;
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime, Space.World);

        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Torreta"))
        {
            other.GetComponent<Torreta>().TomarDaño(daño);
            Destroy(gameObject);
        }
    }
}
