using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [SerializeField] private int maxBodies = 5;
    [SerializeField] private string menuSceneName = "MenuPrincipal"; // Nombre de la escena del menú principal
    private Queue<GameObject> bodies = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    // Este método se llama cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == menuSceneName) // Usa el nombre exacto de tu escena de menú
        {
            ClearAll();
        }
    }

    // Este método se llama cuando el objeto es destruido
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Este método se llama para registrar una muerte
    public void RegisterDeath(GameObject prefab, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        DontDestroyOnLoad(instance);

        bodies.Enqueue(instance);

        if (bodies.Count > maxBodies)
        {
            GameObject oldest = bodies.Dequeue();
            if (oldest != null) Destroy(oldest);
        }
    }

    // Este método se llama para eliminar todos los cuerpos
    public void ClearAll()
    {
        foreach (var obj in bodies)
        {
            if (obj != null) Destroy(obj);
        }
        bodies.Clear();
    }
}
