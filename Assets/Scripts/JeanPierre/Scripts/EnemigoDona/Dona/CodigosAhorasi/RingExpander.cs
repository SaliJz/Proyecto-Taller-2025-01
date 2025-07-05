using UnityEngine;
using System.Collections.Generic;

public class RingExpander : MonoBehaviour
{
    [Header("Prefab y configuraci�n del anillo")]
    [Tooltip("Prefab que se instanciar� formando el anillo")]
    public GameObject prefab;

    [Tooltip("N�mero de objetos en el anillo")]
    public int count = 12;

    [Tooltip("Radio inicial del anillo")]
    public float initialRadius = 2f;

    [Header("Movimiento")]
    [Tooltip("Velocidad a la que los objetos se alejan del centro")]
    public float moveSpeed = 1f;

    [Header("Escalado")]
    [Tooltip("Velocidad de escalado en el eje Z (unidades por segundo)")]
    public float scaleSpeed = 0.1f;

    [Header("Destrucci�n")]
    [Tooltip("Tiempo (en segundos) antes de que cada objeto se destruya")]
    public float lifetime = 5f;

    private List<Transform> ringObjects = new List<Transform>();

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("RingExpander: falta asignar el prefab.");
            enabled = false;
            return;
        }

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * initialRadius,
                0f,
                Mathf.Sin(angle) * initialRadius
            );

            GameObject go = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);

            // Alinea la derecha local con la direcci�n exterior
            Vector3 outwardDir = offset.normalized;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.right, outwardDir);

            // Conserva la escala inicial (se leer� en Update para escalar suavemente)
            ringObjects.Add(go.transform);

            // Programa la autodestrucci�n tras 'lifetime' segundos
            Destroy(go, lifetime);
        }
    }

    void Update()
    {
        float delta = Time.deltaTime;

        foreach (Transform t in ringObjects)
        {
            if (t == null) continue;

            // Mueve cada objeto en la direcci�n de su derecha local
            t.position += t.right * moveSpeed * delta;

            // Escala suavemente en Z, partiendo del valor actual
            Vector3 s = t.localScale;
            s.z += scaleSpeed * delta;
            t.localScale = s;
        }
    }
}




//using UnityEngine;
//using System.Collections.Generic;

//public class RingExpander : MonoBehaviour
//{
//    [Header("Prefab y configuraci�n del anillo")]
//    [Tooltip("Prefab que se instanciar� formando el anillo")]
//    public GameObject prefab;

//    [Tooltip("N�mero de objetos en el anillo")]
//    public int count = 12;

//    [Tooltip("Radio inicial del anillo")]
//    public float initialRadius = 2f;

//    [Header("Movimiento")]
//    [Tooltip("Velocidad a la que los objetos se alejan del centro")]
//    public float moveSpeed = 1f;

//    [Header("Destrucci�n")]
//    [Tooltip("Tiempo (en segundos) antes de que cada objeto se destruya")]
//    public float lifetime = 5f;

//    private List<Transform> ringObjects = new List<Transform>();

//    void Start()
//    {
//        if (prefab == null)
//        {
//            Debug.LogError("RingExpander: falta asignar el prefab.");
//            enabled = false;
//            return;
//        }

//        float angleStep = 360f / count;
//        for (int i = 0; i < count; i++)
//        {
//            float angle = Mathf.Deg2Rad * (i * angleStep);
//            Vector3 offset = new Vector3(
//                Mathf.Cos(angle) * initialRadius,
//                0f,
//                Mathf.Sin(angle) * initialRadius
//            );

//            GameObject go = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);

//            // Alinea la derecha local con la direcci�n exterior
//            Vector3 outwardDir = offset.normalized;
//            go.transform.rotation = Quaternion.FromToRotation(Vector3.right, outwardDir);

//            ringObjects.Add(go.transform);

//            // Programa la autodestrucci�n tras 'lifetime' segundos
//            Destroy(go, lifetime);
//        }
//    }

//    void Update()
//    {
//        // Mueve cada objeto en la direcci�n de su derecha local
//        foreach (Transform t in ringObjects)
//        {
//            if (t != null) // comprueba que no haya sido destruido a�n
//                t.position += t.right * moveSpeed * Time.deltaTime;
//        }
//    }
//}
