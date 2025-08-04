using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [Header("Configuración de Cuerpos")]
    [SerializeField] private int maxBodies = 5;

    [Header("Configuración de Escena")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private string mainMenuSceneName = "MenuPrincipal";

    [Header("Configuración de Colocación")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float placementSphereRadius = 0.5f;
    [SerializeField] private float maxGroundingDistance = 100f;

    private class BodyData
    {
        public GameObject body;
        public string sceneName;
    }

    private Queue<BodyData> bodies = new Queue<BodyData>();
    private int bodyCounter = 0;

    private void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentSceneName = scene.name;
        var remainingBodies = new Queue<BodyData>();

        foreach (var bodyData in bodies)
        {
            if (bodyData.body == null) continue;

            bool isSpecialScene = (currentSceneName == mainMenuSceneName || currentSceneName == gameOverSceneName);

            if (isSpecialScene)
            {
                bodyData.body.SetActive(false); // Oculta el cuerpo si es una escena especial
                remainingBodies.Enqueue(bodyData);
            }
            else if (bodyData.sceneName != currentSceneName)
            {
                bodyData.body.SetActive(false); // Oculta el cuerpo si no es de la escena actual
                remainingBodies.Enqueue(bodyData);
            }
            else
            {
                bodyData.body.SetActive(true); // Activa el cuerpo si es de la escena actual
                remainingBodies.Enqueue(bodyData);
            }
        }

        bodies = remainingBodies;
    }

    public void RegisterDeath(GameObject prefab, Vector3 position)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        Vector3 rayOrigin = position + Vector3.up * placementSphereRadius;

        if (Physics.SphereCast(rayOrigin, placementSphereRadius, Vector3.down, out RaycastHit hit, maxGroundingDistance, groundLayerMask))
        {
            Vector3 finalPosition = hit.point + hit.normal * placementSphereRadius;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            GameObject instance = Instantiate(prefab, finalPosition, rotation);
            DontDestroyOnLoad(instance);

            bodyCounter++;

            DeathBodyController controller = instance.GetComponent<DeathBodyController>();
            if (controller != null)
            {
                controller.SetText($"Cuerpo #{bodyCounter}");
            }

            bodies.Enqueue(new BodyData { body = instance, sceneName = currentSceneName });

            if (bodies.Count > maxBodies)
            {
                BodyData oldest = bodies.Dequeue();
                if (oldest.body != null)
                {
                    Destroy(oldest.body);
                }
            }
        }
        else
        {
            Debug.LogWarning("DeathManager: No se encontró suelo para colocar el cuerpo.");
        }
    }

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
}
