using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [SerializeField] private int maxBodies = 5;
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private string mainMenuSceneName = "MenuPrincipal";

    private class BodyData
    {
        public GameObject body;
        public string sceneName;
    }

    private Queue<BodyData> bodies = new Queue<BodyData>();
    private int bodyCounter = 0;

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

        foreach (var bodyData in bodies)
        {
            if (bodyData.body == null) continue;

            bool isSpecialScene = (currentSceneName == mainMenuSceneName || currentSceneName == gameOverSceneName);

            if (isSpecialScene || bodyData.sceneName != currentSceneName)
            {
                bodyData.body.SetActive(false); // Oculta el cuerpo
            }
            else
            {
                bodyData.body.SetActive(true); // Muestra el cuerpo
            }
        }
    }

    public void RegisterDeath(GameObject prefab, Vector3 position)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        RaycastHit hit;
        Vector3 rayOrigin = position + Vector3.up;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
        {
            Vector3 surfaceNormal = hit.normal;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);

            GameObject instance = Instantiate(prefab, hit.point, rotation);
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
