using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class CobraPoseController : MonoBehaviour
{
    public enum Estado { Inactivo, Pose, Atacando, Retornando }

    [Header("Pose de cobra")]
    public bool poseOnStart = false;
    public float alturaMax = 2f;
    [Range(0, 180)] public float anguloCurva = 90f;
    [Min(2)] public int segmentosCuello = 8;
    public float poseSmoothSpeed = 10f;

    [Header("Secuencia automática")]
    public float delayAntesDePose = 1f;
    public float delayAntesDeAtaque = 1.5f;
    public float delayAntesDeDesactivar = 1f;
    public float distanciaActivacion = 10f;

    private SnakeController snake;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform player;
    private bool sequenceRunning = false;
    private bool hasTriggered = false;

    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
    private float timer;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
        else player = p.transform;

        if (poseOnStart) EntrarPose();
    }

    void Update()
    {
        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
        if (player == null) return;

        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
        {
            hasTriggered = true;
            StartCoroutine(FullSequence());
        }
        else if (dist > distanciaActivacion)
        {
            hasTriggered = false;
        }

        switch (estado)
        {
            case Estado.Pose:
                AplicarPose();
                break;
            case Estado.Atacando:
                UpdateAtaque();
                break;
            case Estado.Retornando:
                UpdateRetorno();
                break;
            case Estado.Inactivo:
                snake.enabled = true;
                break;
        }
    }

    private IEnumerator FullSequence()
    {
        sequenceRunning = true;
        yield return new WaitForSeconds(delayAntesDePose);
        EntrarPose();
        yield return new WaitForSeconds(delayAntesDeAtaque);
        IniciarAtaque();
        // Esperamos a que el ataque y el retorno se completen (cuando el estado vuelva a Pose)
        while (estado != Estado.Pose) yield return null;
        yield return new WaitForSeconds(delayAntesDeDesactivar);
        SalirPose();
        sequenceRunning = false;
        hasTriggered = false;
    }

    void EntrarPose()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
        estado = Estado.Pose;
        snake.enabled = false;
    }

    void SalirPose() => estado = Estado.Inactivo;

    void AplicarPose()
    {
        if (segmentos == null) return;
        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
            ? transform.forward
            : (playerFlat - headFlat).normalized;
        Vector3 targetHead = headFlat + Vector3.up * alturaMax;

        // **MODIFICACIÓN INICIO**
        // Orientar la cabeza para que mire al jugador horizontalmente
        Transform cabeza = segmentos[0];
        Vector3 lookDirToPlayer = (playerFlat - cabeza.position);
        lookDirToPlayer.y = 0; // Mantener la orientación horizontal
        if (lookDirToPlayer.sqrMagnitude > 0.001f)
        {
            cabeza.rotation = Quaternion.Slerp(
                cabeza.rotation,
                Quaternion.LookRotation(lookDirToPlayer),
                poseSmoothSpeed * Time.deltaTime
            );
        }
        // **MODIFICACIÓN FIN**

        FollowChain(targetHead, dir, poseSmoothSpeed);
    }

    void IniciarAtaque()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

        headAttackStart = segmentos[0].position;
        baseAttackStart = segmentos[segmentosCuello].position; // La base del cuello es el último segmento del cuello

        // Calculamos la dirección hacia el jugador desde la cabeza (para un ataque más dirigido)
        Vector3 rawDir = (player.position - headAttackStart).normalized;
        // Proyectamos el target a una distancia razonable desde la base
        attackTarget = headAttackStart + rawDir * (Vector3.Distance(headAttackStart, baseAttackStart) * 1.5f); // Un poco más allá de la distancia inicial

        // Orientamos la cabeza localmente hacia el punto de ataque (solo ese momento)
        Transform cabeza = segmentos[0];
        Vector3 lookDir = (attackTarget - cabeza.position);
        lookDir.y = 0; // Mantener la orientación horizontal
        if (lookDir.sqrMagnitude > 0.001f)
            cabeza.rotation = Quaternion.LookRotation(lookDir);

        timer = 0f;
        estado = Estado.Atacando;
    }

    void UpdateAtaque()
    {
        // Usamos un tiempo para la interpolación que puede ser diferente al delay
        float attackDuration = 0.5f; // Duración del ataque
        timer = Mathf.Min(1f, timer + Time.deltaTime / attackDuration);

        // Interpolación de la posición de la cabeza
        Vector3 currentHeadPos = Vector3.Lerp(headAttackStart, attackTarget, timer);
        // Curva de arco para la altura de la cabeza
        float arcHeight = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.7f; // Ajusta el pico del arco
        currentHeadPos.y += arcHeight;

        // Calculamos la base del cuello actual (se mantiene relativamente fija o se mueve un poco con la serpiente)
        // Podríamos considerar que la base del cuello también se mueve hacia adelante si el ataque es muy pronunciado
        // Para simplificar, la mantenemos cerca de baseAttackStart
        Vector3 currentBasePos = baseAttackStart; // Por ahora, la base del cuello no se mueve mucho

        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f); // El control de altura sigue siendo relevante para la curva

        if (timer >= 1f)
        {
            headAttackEnd = segmentos[0].position; // Registra la posición final del ataque para el retorno
            timer = 0f;
            estado = Estado.Retornando;
        }
    }

    void UpdateRetorno()
    {
        // Duración del retorno
        float returnDuration = 0.7f;
        timer = Mathf.Min(1f, timer + Time.deltaTime / returnDuration);

        // La cabeza retorna a la posición de pose, que es directamente sobre la base del cuello
        Vector3 targetReturnHead = baseAttackStart + Vector3.up * alturaMax;
        Vector3 currentHeadPos = Vector3.Lerp(headAttackEnd, targetReturnHead, timer);

        Vector3 currentBasePos = baseAttackStart; // La base del cuello permanece en su lugar

        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f);

        if (timer >= 1f)
        {
            estado = Estado.Pose; // Vuelve al estado de pose cuando termina el retorno
        }
    }

    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float controlHeight)
    {
        // Calcula la longitud total del "cuello" en base a la distancia entre headPos y basePos
        float neckLength = Vector3.Distance(headPos, basePos);

        // El segmento 0 (cabeza) siempre va en headPos
        segmentos[0].position = headPos;

        // Calcula la dirección principal del cuello (desde la base hacia la cabeza)
        Vector3 mainDir = (headPos - basePos).normalized;

        for (int i = 1; i < segmentosCuello; i++)
        {
            // t representa la "proporción" a lo largo del cuello, desde la cabeza (0) hasta la base (1)
            // Lo invertimos para que el segmento 1 sea el más cercano a la cabeza y segmentosCuello-1 el más cercano a la base
            float t = (float)i / (segmentosCuello - 1);

            // Interpolamos la posición linealmente entre la cabeza y la base
            Vector3 linearPos = Vector3.Lerp(headPos, basePos, t);

            // Aplicamos una curva vertical para dar la forma de cuello de cobra
            // La curva es más pronunciada en el medio del cuello
            float arcFactor = Mathf.Sin(t * Mathf.PI); // Sin(0)=0, Sin(PI/2)=1, Sin(PI)=0
            Vector3 curvedPos = linearPos + Vector3.up * controlHeight * arcFactor;

            segmentos[i].position = curvedPos;

            // Orientar el segmento para que "mire" al siguiente segmento o a la cabeza
            Vector3 lookTarget = (i < segmentosCuello - 1) ? segmentos[i + 1].position : basePos; // Mira al siguiente o a la base si es el último
            Vector3 lookDir = lookTarget - segmentos[i].position;

            // Queremos que la orientación sea principalmente horizontal
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(lookDir),
                    poseSmoothSpeed * Time.deltaTime // Usa la velocidad de suavizado de pose
                );
            }
        }
    }

    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
    {
        float sep = snake.separacionSegmentos;
        float halfPi = Mathf.PI * 0.5f;
        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
        Vector3 prev = segmentos[0].position;
        for (int i = 1; i < segmentos.Count; i++)
        {
            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
            Vector3 bent = (i < segmentosCuello)
                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
                : dirXZ;
            float yOff = (i < segmentosCuello)
                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
                : 0f;
            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
            Vector3 target = prev - bentXZ * sep;
            target.y = yOff;
            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
            Vector3 look = prev - segmentos[i].position; look.y = 0;
            if (look.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(look),
                    speed * Time.deltaTime
                );
            prev = segmentos[i].position;
        }
    }
}






// //masactual

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Atacando, Retornando }

//    [Header("Pose de cobra")]
//    public bool poseOnStart = false;
//    public float alturaMax = 2f;
//    [Range(0, 180)] public float anguloCurva = 90f;
//    [Min(2)] public int segmentosCuello = 8;
//    public float poseSmoothSpeed = 10f;

//    [Header("Secuencia automática")]
//    public float delayAntesDePose = 1f;
//    public float delayAntesDeAtaque = 1.5f;
//    public float delayAntesDeDesactivar = 1f;
//    public float distanciaActivacion = 10f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform player;
//    private bool sequenceRunning = false;
//    private bool hasTriggered = false;

//    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
//    private float timer;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
//        else player = p.transform;

//        if (poseOnStart) EntrarPose();
//    }

//    void Update()
//    {
//        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
//        if (player == null) return;

//        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

//        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
//        {
//            hasTriggered = true;
//            StartCoroutine(FullSequence());
//        }
//        else if (dist > distanciaActivacion)
//        {
//            hasTriggered = false;
//        }

//        switch (estado)
//        {
//            case Estado.Pose:
//                AplicarPose();
//                break;
//            case Estado.Atacando:
//                UpdateAtaque();
//                break;
//            case Estado.Retornando:
//                UpdateRetorno();
//                break;
//            case Estado.Inactivo:
//                snake.enabled = true;
//                break;
//        }
//    }

//    private IEnumerator FullSequence()
//    {
//        sequenceRunning = true;
//        yield return new WaitForSeconds(delayAntesDePose);
//        EntrarPose();
//        yield return new WaitForSeconds(delayAntesDeAtaque);
//        IniciarAtaque();
//        // Esperamos a que el ataque y el retorno se completen (cuando el estado vuelva a Pose)
//        while (estado != Estado.Pose) yield return null;
//        yield return new WaitForSeconds(delayAntesDeDesactivar);
//        SalirPose();
//        sequenceRunning = false;
//        hasTriggered = false;
//    }

//    void EntrarPose()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        snake.enabled = false;
//    }

//    void SalirPose() => estado = Estado.Inactivo;

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (playerFlat - headFlat).normalized;
//        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
//        FollowChain(targetHead, dir, poseSmoothSpeed);
//    }

//    void IniciarAtaque()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

//        headAttackStart = segmentos[0].position;
//        baseAttackStart = segmentos[segmentosCuello].position; // La base del cuello es el último segmento del cuello

//        // Calculamos la dirección hacia el jugador desde la cabeza (para un ataque más dirigido)
//        Vector3 rawDir = (player.position - headAttackStart).normalized;
//        // Proyectamos el target a una distancia razonable desde la base
//        attackTarget = headAttackStart + rawDir * (Vector3.Distance(headAttackStart, baseAttackStart) * 1.5f); // Un poco más allá de la distancia inicial

//        // Orientamos la cabeza localmente hacia el punto de ataque (solo ese momento)
//        Transform cabeza = segmentos[0];
//        Vector3 lookDir = (attackTarget - cabeza.position);
//        lookDir.y = 0; // Mantener la orientación horizontal
//        if (lookDir.sqrMagnitude > 0.001f)
//            cabeza.rotation = Quaternion.LookRotation(lookDir);

//        timer = 0f;
//        estado = Estado.Atacando;
//    }

//    void UpdateAtaque()
//    {
//        // Usamos un tiempo para la interpolación que puede ser diferente al delay
//        float attackDuration = 0.5f; // Duración del ataque
//        timer = Mathf.Min(1f, timer + Time.deltaTime / attackDuration);

//        // Interpolación de la posición de la cabeza
//        Vector3 currentHeadPos = Vector3.Lerp(headAttackStart, attackTarget, timer);
//        // Curva de arco para la altura de la cabeza
//        float arcHeight = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.7f; // Ajusta el pico del arco
//        currentHeadPos.y += arcHeight;

//        // Calculamos la base del cuello actual (se mantiene relativamente fija o se mueve un poco con la serpiente)
//        // Podríamos considerar que la base del cuello también se mueve hacia adelante si el ataque es muy pronunciado
//        // Para simplificar, la mantenemos cerca de baseAttackStart
//        Vector3 currentBasePos = baseAttackStart; // Por ahora, la base del cuello no se mueve mucho

//        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f); // El control de altura sigue siendo relevante para la curva

//        if (timer >= 1f)
//        {
//            headAttackEnd = segmentos[0].position; // Registra la posición final del ataque para el retorno
//            timer = 0f;
//            estado = Estado.Retornando;
//        }
//    }

//    void UpdateRetorno()
//    {
//        // Duración del retorno
//        float returnDuration = 0.7f;
//        timer = Mathf.Min(1f, timer + Time.deltaTime / returnDuration);

//        // La cabeza retorna a la posición de pose, que es directamente sobre la base del cuello
//        Vector3 targetReturnHead = baseAttackStart + Vector3.up * alturaMax;
//        Vector3 currentHeadPos = Vector3.Lerp(headAttackEnd, targetReturnHead, timer);

//        Vector3 currentBasePos = baseAttackStart; // La base del cuello permanece en su lugar

//        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f);

//        if (timer >= 1f)
//        {
//            estado = Estado.Pose; // Vuelve al estado de pose cuando termina el retorno
//        }
//    }

//    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float controlHeight)
//    {
//        // Calcula la longitud total del "cuello" en base a la distancia entre headPos y basePos
//        float neckLength = Vector3.Distance(headPos, basePos);

//        // El segmento 0 (cabeza) siempre va en headPos
//        segmentos[0].position = headPos;

//        // Calcula la dirección principal del cuello (desde la base hacia la cabeza)
//        Vector3 mainDir = (headPos - basePos).normalized;

//        for (int i = 1; i < segmentosCuello; i++)
//        {
//            // t representa la "proporción" a lo largo del cuello, desde la cabeza (0) hasta la base (1)
//            // Lo invertimos para que el segmento 1 sea el más cercano a la cabeza y segmentosCuello-1 el más cercano a la base
//            float t = (float)i / (segmentosCuello - 1);

//            // Interpolamos la posición linealmente entre la cabeza y la base
//            Vector3 linearPos = Vector3.Lerp(headPos, basePos, t);

//            // Aplicamos una curva vertical para dar la forma de cuello de cobra
//            // La curva es más pronunciada en el medio del cuello
//            float arcFactor = Mathf.Sin(t * Mathf.PI); // Sin(0)=0, Sin(PI/2)=1, Sin(PI)=0
//            Vector3 curvedPos = linearPos + Vector3.up * controlHeight * arcFactor;

//            segmentos[i].position = curvedPos;

//            // Orientar el segmento para que "mire" al siguiente segmento o a la cabeza
//            Vector3 lookTarget = (i < segmentosCuello - 1) ? segmentos[i + 1].position : basePos; // Mira al siguiente o a la base si es el último
//            Vector3 lookDir = lookTarget - segmentos[i].position;

//            // Queremos que la orientación sea principalmente horizontal
//            lookDir.y = 0;

//            if (lookDir.sqrMagnitude > 0.001f)
//            {
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(lookDir),
//                    poseSmoothSpeed * Time.deltaTime // Usa la velocidad de suavizado de pose
//                );
//            }
//        }
//    }

//    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
//    {
//        float sep = snake.separacionSegmentos;
//        float halfPi = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
//        Vector3 prev = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
//                : 0f;
//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 target = prev - bentXZ * sep;
//            target.y = yOff;
//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
//            Vector3 look = prev - segmentos[i].position; look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    speed * Time.deltaTime
//                );
//            prev = segmentos[i].position;
//        }
//    }
//}



















//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Atacando, Retornando }

//    [Header("Pose de cobra")]
//    public bool poseOnStart = false;
//    public float alturaMax = 2f;
//    [Range(0, 180)] public float anguloCurva = 90f;
//    [Min(2)] public int segmentosCuello = 8;
//    public float poseSmoothSpeed = 10f;

//    [Header("Secuencia automática")]
//    public float delayAntesDePose = 1f;
//    public float delayAntesDeAtaque = 1.5f;
//    public float delayAntesDeDesactivar = 1f;
//    public float distanciaActivacion = 10f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform player;
//    private bool sequenceRunning = false;
//    private bool hasTriggered = false;

//    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
//    private float timer;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
//        else player = p.transform;

//        if (poseOnStart) EntrarPose();
//    }

//    void Update()
//    {
//        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
//        if (player == null) return;

//        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

//        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
//        {
//            hasTriggered = true;
//            StartCoroutine(FullSequence());
//        }
//        else if (dist > distanciaActivacion)
//        {
//            hasTriggered = false;
//        }

//        switch (estado)
//        {
//            case Estado.Pose:
//                AplicarPose();
//                break;
//            case Estado.Atacando:
//                UpdateAtaque();
//                break;
//            case Estado.Retornando:
//                UpdateRetorno();
//                break;
//            case Estado.Inactivo:
//                snake.enabled = true;
//                break;
//        }
//    }

//    private IEnumerator FullSequence()
//    {
//        sequenceRunning = true;
//        yield return new WaitForSeconds(delayAntesDePose);
//        EntrarPose();
//        yield return new WaitForSeconds(delayAntesDeAtaque);
//        IniciarAtaque();
//        // Esperamos a que el ataque y el retorno se completen (cuando el estado vuelva a Pose)
//        while (estado != Estado.Pose) yield return null;
//        yield return new WaitForSeconds(delayAntesDeDesactivar);
//        SalirPose();
//        sequenceRunning = false;
//        hasTriggered = false;
//    }

//    void EntrarPose()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        snake.enabled = false;
//    }

//    void SalirPose() => estado = Estado.Inactivo;

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (playerFlat - headFlat).normalized;
//        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
//        FollowChain(targetHead, dir, poseSmoothSpeed);
//    }

//    void IniciarAtaque()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

//        headAttackStart = segmentos[0].position;
//        baseAttackStart = segmentos[segmentosCuello].position; // La base del cuello es el último segmento del cuello

//        // Calculamos la dirección hacia el jugador desde la cabeza (para un ataque más dirigido)
//        Vector3 rawDir = (player.position - headAttackStart).normalized;
//        // Proyectamos el target a una distancia razonable desde la base
//        attackTarget = headAttackStart + rawDir * (Vector3.Distance(headAttackStart, baseAttackStart) * 1.5f); // Un poco más allá de la distancia inicial

//        // Orientamos la cabeza localmente hacia el punto de ataque (solo ese momento)
//        Transform cabeza = segmentos[0];
//        Vector3 lookDir = (attackTarget - cabeza.position);
//        lookDir.y = 0; // Mantener la orientación horizontal
//        if (lookDir.sqrMagnitude > 0.001f)
//            cabeza.rotation = Quaternion.LookRotation(lookDir);

//        timer = 0f;
//        estado = Estado.Atacando;
//    }

//    void UpdateAtaque()
//    {
//        // Usamos un tiempo para la interpolación que puede ser diferente al delay
//        float attackDuration = 0.5f; // Duración del ataque
//        timer = Mathf.Min(1f, timer + Time.deltaTime / attackDuration);

//        // Interpolación de la posición de la cabeza
//        Vector3 currentHeadPos = Vector3.Lerp(headAttackStart, attackTarget, timer);
//        // Curva de arco para la altura de la cabeza
//        float arcHeight = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.7f; // Ajusta el pico del arco
//        currentHeadPos.y += arcHeight;

//        // Calculamos la base del cuello actual (se mantiene relativamente fija o se mueve un poco con la serpiente)
//        // Podríamos considerar que la base del cuello también se mueve hacia adelante si el ataque es muy pronunciado
//        // Para simplificar, la mantenemos cerca de baseAttackStart
//        Vector3 currentBasePos = baseAttackStart; // Por ahora, la base del cuello no se mueve mucho

//        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f); // El control de altura sigue siendo relevante para la curva

//        if (timer >= 1f)
//        {
//            headAttackEnd = segmentos[0].position; // Registra la posición final del ataque para el retorno
//            timer = 0f;
//            estado = Estado.Retornando;
//        }
//    }

//    void UpdateRetorno()
//    {
//        // Duración del retorno
//        float returnDuration = 0.7f;
//        timer = Mathf.Min(1f, timer + Time.deltaTime / returnDuration);

//        // La cabeza retorna a la posición de pose, que es directamente sobre la base del cuello
//        Vector3 targetReturnHead = baseAttackStart + Vector3.up * alturaMax;
//        Vector3 currentHeadPos = Vector3.Lerp(headAttackEnd, targetReturnHead, timer);

//        Vector3 currentBasePos = baseAttackStart; // La base del cuello permanece en su lugar

//        MoveNeckCurved(currentHeadPos, currentBasePos, alturaMax * 0.5f);

//        if (timer >= 1f)
//        {
//            estado = Estado.Pose; // Vuelve al estado de pose cuando termina el retorno
//        }
//    }

//    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float controlHeight)
//    {
//        // Calcula la longitud total del "cuello" en base a la distancia entre headPos y basePos
//        float neckLength = Vector3.Distance(headPos, basePos);

//        // El segmento 0 (cabeza) siempre va en headPos
//        segmentos[0].position = headPos;

//        // Calcula la dirección principal del cuello (desde la base hacia la cabeza)
//        Vector3 mainDir = (headPos - basePos).normalized;

//        for (int i = 1; i < segmentosCuello; i++)
//        {
//            // t representa la "proporción" a lo largo del cuello, desde la cabeza (0) hasta la base (1)
//            // Lo invertimos para que el segmento 1 sea el más cercano a la cabeza y segmentosCuello-1 el más cercano a la base
//            float t = (float)i / (segmentosCuello - 1);

//            // Interpolamos la posición linealmente entre la cabeza y la base
//            Vector3 linearPos = Vector3.Lerp(headPos, basePos, t);

//            // Aplicamos una curva vertical para dar la forma de cuello de cobra
//            // La curva es más pronunciada en el medio del cuello
//            float arcFactor = Mathf.Sin(t * Mathf.PI); // Sin(0)=0, Sin(PI/2)=1, Sin(PI)=0
//            Vector3 curvedPos = linearPos + Vector3.up * controlHeight * arcFactor;

//            segmentos[i].position = curvedPos;

//            // Orientar el segmento para que "mire" al siguiente segmento o a la cabeza
//            Vector3 lookTarget = (i < segmentosCuello - 1) ? segmentos[i + 1].position : basePos; // Mira al siguiente o a la base si es el último
//            Vector3 lookDir = lookTarget - segmentos[i].position;

//            // Queremos que la orientación sea principalmente horizontal
//            lookDir.y = 0;

//            if (lookDir.sqrMagnitude > 0.001f)
//            {
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(lookDir),
//                    poseSmoothSpeed * Time.deltaTime // Usa la velocidad de suavizado de pose
//                );
//            }
//        }
//    }

//    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
//    {
//        float sep = snake.separacionSegmentos;
//        float halfPi = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
//        Vector3 prev = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
//                : 0f;
//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 target = prev - bentXZ * sep;
//            target.y = yOff;
//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
//            Vector3 look = prev - segmentos[i].position; look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    speed * Time.deltaTime
//                );
//            prev = segmentos[i].position;
//        }
//    }
//}















































//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Atacando, Retornando }

//    [Header("Pose de cobra")]
//    public bool poseOnStart = false;
//    public float alturaMax = 2f;
//    [Range(0, 180)] public float anguloCurva = 90f;
//    [Min(2)] public int segmentosCuello = 8;
//    public float poseSmoothSpeed = 10f;

//    [Header("Secuencia automática")]
//    public float delayAntesDePose = 1f;
//    public float delayAntesDeAtaque = 1.5f;
//    public float delayAntesDeDesactivar = 1f;
//    public float distanciaActivacion = 10f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform player;
//    private bool sequenceRunning = false;
//    private bool hasTriggered = false;

//    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
//    private float timer;
//    private float neckLength; // ← distancia máxima cabeza–base

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
//        else player = p.transform;

//        if (poseOnStart) EntrarPose();
//    }

//    void Update()
//    {
//        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
//        if (player == null) return;

//        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

//        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
//        {
//            hasTriggered = true;
//            StartCoroutine(FullSequence());
//        }
//        else if (dist > distanciaActivacion)
//        {
//            hasTriggered = false;
//        }

//        switch (estado)
//        {
//            case Estado.Pose:
//                AplicarPose();
//                break;
//            case Estado.Atacando:
//                UpdateAtaque();
//                break;
//            case Estado.Retornando:
//                UpdateRetorno();
//                break;
//            case Estado.Inactivo:
//                snake.enabled = true;
//                break;
//        }
//    }

//    private IEnumerator FullSequence()
//    {
//        sequenceRunning = true;
//        yield return new WaitForSeconds(delayAntesDePose);
//        EntrarPose();
//        yield return new WaitForSeconds(delayAntesDeAtaque);
//        IniciarAtaque();
//        while (estado != Estado.Pose) yield return null;
//        yield return new WaitForSeconds(delayAntesDeDesactivar);
//        SalirPose();
//        sequenceRunning = false;
//        hasTriggered = false;
//    }

//    void EntrarPose()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        snake.enabled = false;
//    }

//    void SalirPose() => estado = Estado.Inactivo;

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (playerFlat - headFlat).normalized;
//        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
//        FollowChain(targetHead, dir, poseSmoothSpeed);
//    }

//    void IniciarAtaque()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

//        headAttackStart = segmentos[0].position;
//        baseAttackStart = segmentos[segmentosCuello].position;

//        // Calculamos la dirección hacia el jugador
//        Vector3 rawDir = (player.position - baseAttackStart).normalized;

//        // Calculamos el punto de ataque
//        attackTarget = baseAttackStart + rawDir * (Vector3.Distance(headAttackStart, baseAttackStart) * 0.8f);

//        timer = 0f;
//        estado = Estado.Atacando;
//    }

//    void UpdateAtaque()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeAtaque);

//        // Lerp horizontal con arco vertical
//        Vector3 flatLerp = Vector3.Lerp(headAttackStart, attackTarget, timer);
//        float arcY = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.5f;
//        Vector3 headPos = flatLerp + Vector3.up * arcY;

//        MoveNeckCurved(headPos, baseAttackStart, alturaMax * 0.5f);

//        if (timer >= 1f)
//        {
//            headAttackEnd = segmentos[0].position;
//            timer = 0f;
//            estado = Estado.Retornando;
//        }
//    }

//    void UpdateRetorno()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeDesactivar);
//        Vector3 returnHead = baseAttackStart + Vector3.up * alturaMax;
//        Vector3 headPos = Vector3.Lerp(headAttackEnd, returnHead, timer);
//        MoveNeckCurved(headPos, baseAttackStart, alturaMax * 0.5f);

//        if (timer >= 1f) estado = Estado.Pose;
//    }

//    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float alturaControl)
//    {
//        Vector3 dir = (headPos - basePos).normalized;
//        Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;

//        segmentos[0].position = headPos;

//        for (int i = 1; i < segmentosCuello; i++)
//        {
//            float t = (float)i / (segmentosCuello - 1);
//            Vector3 pos = Vector3.Lerp(basePos, headPos, t);
//            pos += Vector3.up * Mathf.Sin(t * Mathf.PI) * alturaControl;

//            segmentos[i].position = pos;

//            Vector3 look = (i < segmentosCuello - 1) ? segmentos[i + 1].position - pos : headPos - pos;
//            look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.LookRotation(look);
//        }
//    }

//    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
//    {
//        float sep = snake.separacionSegmentos;
//        float halfPi = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
//        Vector3 prev = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
//                : 0f;
//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 target = prev - bentXZ * sep;
//            target.y = yOff;
//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
//            Vector3 look = prev - segmentos[i].position; look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    speed * Time.deltaTime
//                );
//            prev = segmentos[i].position;
//        }
//    }
//}



































//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Atacando, Retornando }

//    [Header("Pose de cobra")]
//    public bool poseOnStart = false;
//    public float alturaMax = 2f;
//    [Range(0, 180)] public float anguloCurva = 90f;
//    [Min(2)] public int segmentosCuello = 8;
//    public float poseSmoothSpeed = 10f;

//    [Header("Secuencia automática")]
//    public float delayAntesDePose = 1f;
//    public float delayAntesDeAtaque = 1.5f;
//    public float delayAntesDeDesactivar = 1f;
//    public float distanciaActivacion = 10f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform player;
//    private bool sequenceRunning = false;
//    private bool hasTriggered = false;

//    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
//    private float timer;
//    private float neckLength; // ← distancia máxima cabeza–base

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
//        else player = p.transform;

//        if (poseOnStart) EntrarPose();
//    }

//    void Update()
//    {
//        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
//        if (player == null) return;

//        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

//        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
//        {
//            hasTriggered = true;
//            StartCoroutine(FullSequence());
//        }
//        else if (dist > distanciaActivacion)
//        {
//            hasTriggered = false;
//        }

//        switch (estado)
//        {
//            case Estado.Pose:
//                AplicarPose();
//                break;
//            case Estado.Atacando:
//                UpdateAtaque();
//                break;
//            case Estado.Retornando:
//                UpdateRetorno();
//                break;
//            case Estado.Inactivo:
//                snake.enabled = true;
//                break;
//        }
//    }

//    private IEnumerator FullSequence()
//    {
//        sequenceRunning = true;
//        yield return new WaitForSeconds(delayAntesDePose);
//        EntrarPose();
//        yield return new WaitForSeconds(delayAntesDeAtaque);
//        IniciarAtaque();
//        while (estado != Estado.Pose) yield return null;
//        yield return new WaitForSeconds(delayAntesDeDesactivar);
//        SalirPose();
//        sequenceRunning = false;
//        hasTriggered = false;
//    }

//    void EntrarPose()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        snake.enabled = false;
//    }

//    void SalirPose() => estado = Estado.Inactivo;

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (playerFlat - headFlat).normalized;
//        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
//        FollowChain(targetHead, dir, poseSmoothSpeed);
//    }

//    void IniciarAtaque()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

//        headAttackStart = segmentos[0].position;
//        baseAttackStart = segmentos[segmentosCuello].position;

//        // Calculamos la longitud máxima del cuello
//        neckLength = Vector3.Distance(headAttackStart, baseAttackStart);

//        // Obtenemos dirección y limitamos target para no sobrepasar neckLength
//        Vector3 rawDir = (player.position - baseAttackStart).normalized;
//        attackTarget = baseAttackStart + rawDir * neckLength;

//        timer = 0f;
//        estado = Estado.Atacando;
//    }

//    void UpdateAtaque()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeAtaque);

//        // Lerp horizontal con arco vertical
//        Vector3 flatLerp = Vector3.Lerp(headAttackStart, attackTarget, timer);
//        float arcY = Mathf.Sin(timer * Mathf.PI) * alturaMax;
//        Vector3 headPos = flatLerp + Vector3.up * arcY;

//        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

//        if (timer >= 1f)
//        {
//            headAttackEnd = segmentos[0].position;
//            timer = 0f;
//            estado = Estado.Retornando;
//        }
//    }

//    void UpdateRetorno()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeDesactivar);
//        Vector3 returnHead = baseAttackStart + Vector3.up * alturaMax;
//        Vector3 headPos = Vector3.Lerp(headAttackEnd, returnHead, timer);
//        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

//        if (timer >= 1f) estado = Estado.Pose;
//    }

//    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float alturaControl)
//    {
//        Vector3 p0 = headPos, p2 = basePos;
//        Vector3 p1 = (p0 + p2) * 0.5f + Vector3.up * alturaControl;
//        for (int i = 0; i < segmentosCuello; i++)
//        {
//            float t = (float)i / (segmentosCuello - 1);
//            Vector3 pos = Mathf.Pow(1 - t, 2) * p0
//                        + 2 * (1 - t) * t * p1
//                        + Mathf.Pow(t, 2) * p2;
//            segmentos[i].position = pos;

//            float tn = Mathf.Min(1f, t + 1f / (segmentosCuello - 1));
//            Vector3 next = Mathf.Pow(1 - tn, 2) * p0
//                         + 2 * (1 - tn) * tn * p1
//                         + Mathf.Pow(tn, 2) * p2;
//            Vector3 dir = next - pos; dir.y = 0;
//            if (dir.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.LookRotation(dir);
//        }
//    }

//    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
//    {
//        float sep = snake.separacionSegmentos;
//        float halfPi = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
//        Vector3 prev = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
//                : 0f;
//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 target = prev - bentXZ * sep;
//            target.y = yOff;
//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
//            Vector3 look = prev - segmentos[i].position; look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    speed * Time.deltaTime
//                );
//            prev = segmentos[i].position;
//        }
//    }
//}


















































