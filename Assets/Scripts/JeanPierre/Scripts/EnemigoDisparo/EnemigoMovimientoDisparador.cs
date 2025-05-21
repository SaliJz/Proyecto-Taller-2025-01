using UnityEngine;

[RequireComponent(typeof(EnemyAbilityReceiver))]
public class EnemigoMovimientoDisparador : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    public Transform jugador;

    [Header("Movimiento relativo al jugador")]
    public float distanciaDeseada = 5f;
    public float velocidadMovimiento = 3f;
    public float margen = 0.5f;

    [Header("Circulación alrededor del jugador")]
    public float velocidadCirculo = 2f;
    public bool sentidoHorario = true;

    private EnemyAbilityReceiver abilityReceiver;

    private void Awake()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();

        if (jugador == null)
        {
            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO != null)
                jugador = jugadorGO.transform;
            else
                Debug.LogWarning($"No se encontró ningún GameObject con tag 'Player' en {name}");
        }
    }

    private void Update()
    {
        if (jugador == null) return;

        float speed = abilityReceiver.CurrentSpeed;
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;
        Vector3 moveDir = dir.normalized;

        if (dist > distanciaDeseada + margen)
        {
            // Se aleja para acercarse al rango óptimo
            transform.position = Vector3.MoveTowards(transform.position,
                                                     transform.position + moveDir,
                                                     speed * Time.deltaTime);
        }
        else if (dist < distanciaDeseada - margen)
        {
            // Retrocede si está demasiado cerca
            transform.position = Vector3.MoveTowards(transform.position,
                                                     transform.position - moveDir,
                                                     speed * Time.deltaTime);
        }
        else
        {
            // En rango óptimo: circula alrededor del jugador
            // Calcula dirección perpendicular al vector hacia el jugador
            Vector3 perp;
            if (sentidoHorario)
                perp = Quaternion.Euler(0, 90f, 0) * moveDir;
            else
                perp = Quaternion.Euler(0, -90f, 0) * moveDir;

            transform.position += perp.normalized * velocidadCirculo * Time.deltaTime;
        }

        // Siempre gira suavemente mirando al jugador
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  targetRot,
                                                  speed * Time.deltaTime);
        }
    }
}



//// EnemigoMovimientoDisparador.cs
//using UnityEngine;

//[RequireComponent(typeof(EnemyAbilityReceiver))]
//public class EnemigoMovimientoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Movimiento relativo al jugador")]
//    public float distanciaDeseada = 5f;
//    public float velocidadMovimiento = 3f;
//    public float margen = 0.5f;

//    private EnemyAbilityReceiver abilityReceiver;

//    private void Awake()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();

//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null)
//                jugador = jugadorGO.transform;
//            else
//                Debug.LogWarning($"No se encontró ningún GameObject con tag 'Player' en {name}");
//        }
//    }

//    private void Update()
//    {
//        if (jugador == null) return;

//        float speed = abilityReceiver.CurrentSpeed;

//        Vector3 dir = jugador.position - transform.position;
//        dir.y = 0f;
//        float dist = dir.magnitude;
//        Vector3 moveDir = dir.normalized;

//        if (dist > distanciaDeseada + margen)
//            transform.position = Vector3.MoveTowards(transform.position,
//                                                     transform.position + moveDir,
//                                                     speed * Time.deltaTime);
//        else if (dist < distanciaDeseada - margen)
//            transform.position = Vector3.MoveTowards(transform.position,
//                                                     transform.position - moveDir,
//                                                     speed * Time.deltaTime);

//        if (dir != Vector3.zero)
//        {
//            var targetRot = Quaternion.LookRotation(dir);
//            transform.rotation = Quaternion.Slerp(transform.rotation,
//                                                  targetRot,
//                                                  speed * Time.deltaTime);
//        }
//    }
//}
