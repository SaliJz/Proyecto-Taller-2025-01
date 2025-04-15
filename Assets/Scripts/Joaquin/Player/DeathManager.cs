using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [SerializeField] private int maxBodies = 5;
    private Queue<GameObject> bodies = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

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

    public void ClearAll()
    {
        foreach (var obj in bodies)
        {
            if (obj != null) Destroy(obj);
        }
        bodies.Clear();
    }
}
