using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TMPro;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [SerializeField] private int maxBodies = 5;
    [SerializeField] private string gameOverSceneName = "GameOver";

    // Estructura para almacenar el cuerpo y la escena donde murió
    private class BodyData
    {
        public GameObject body;
        public string sceneName;
    }

    private Queue<BodyData> bodies = new Queue<BodyData>();
    private int bodyCounter = 0; // Contador global para los cuerpos

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Log($"Se destruyó una instancia duplicada de DeathManager: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    // Este método se llama cuando el objeto es destruido
    private void OnDestroy()
    {
        // Desuscribirse del evento de carga de escena
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Este método se llama cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Filtrar los cuerpos para que solo permanezcan los de la escena actual o la de derrota
        string currentSceneName = scene.name;
        var remainingBodies = new Queue<BodyData>();

        foreach (var bodyData in bodies)
        {
            // Si el cuerpo pertenece a la escena actual o es la escena de derrota, lo mantenemos
            if (bodyData.sceneName == currentSceneName || currentSceneName == gameOverSceneName)
            {
                remainingBodies.Enqueue(bodyData);
            }
            else
            {
                if (bodyData.body != null)
                {
                    Log($"Eliminando cuerpo de la escena: {bodyData.sceneName}");
                    Destroy(bodyData.body);
                }
            }
        }

        bodies = remainingBodies;
    }

    // Este método se llama para registrar una muerte
    public void RegisterDeath(GameObject prefab, Vector3 position)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Realizar un raycast hacia abajo para detectar la superficie
        RaycastHit hit;
        Vector3 rayOrigin = position + Vector3.up; // Lanza el raycast desde un poco más arriba
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
        {
            // Obtener la normal de la superficie
            Vector3 surfaceNormal = hit.normal;

            // Calcular la rotación del DeathBody para alinearlo con la superficie
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);

            // Instanciar el DeathBody con la rotación calculada
            GameObject instance = Instantiate(prefab, hit.point, rotation);
            Log($"Cuerpo registrado: {instance.name} en la posición {hit.point} en la escena {currentSceneName}");
            DontDestroyOnLoad(instance);

            // Incrementar el contador de cuerpos
            bodyCounter++;

            // Actualizar el texto del cuerpo
            DeathBodyController controller = instance.GetComponent<DeathBodyController>();
            if (controller != null)
            {
                controller.SetText($"Cuerpo #{bodyCounter}");
            }
            else
            {
                Log("No se encontró el script DeathBodyController en el prefab.");
            }

            // Agregar el cuerpo a la cola
            bodies.Enqueue(new BodyData { body = instance, sceneName = currentSceneName });

            // Eliminar el cuerpo más antiguo si se excede el límite
            if (bodies.Count > maxBodies)
            {
                BodyData oldest = bodies.Dequeue();
                if (oldest.body != null)
                {
                    Log($"Cuerpo eliminado: {oldest.body.name} de la escena {oldest.sceneName}");
                    Destroy(oldest.body);
                }
            }
        }
        else
        {
            Log("No se detectó ninguna superficie debajo del punto especificado.");
        }
    }

    // Este método se llama para eliminar todos los cuerpos
    public void ClearAll()
    {
        foreach (var bodyData in bodies)
        {
            if (bodyData.body != null)
            {
                Destroy(bodyData.body);
            }
        }
        bodies.Clear();
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}
