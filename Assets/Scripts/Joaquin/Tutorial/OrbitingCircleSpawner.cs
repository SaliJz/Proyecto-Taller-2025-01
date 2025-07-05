using UnityEngine;
using System.Collections.Generic;

public class OrbitingCircleSpawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform target;
    public int numberOfObjects = 12;
    public float radius = 5f;
    public float rotationSpeed = 30f;

    public bool activateScripts = false; // <-- booleano de activación

    private Transform orbitCenter;
    private List<GameObject> spawnedObjects = new List<GameObject>(); // lista para guardar referencias

    void Start()
    {
        orbitCenter = new GameObject("OrbitCenter").transform;
        orbitCenter.position = target.position;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = (360f / numberOfObjects) * i;
            float rad = Mathf.Deg2Rad * angle;

            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;

            Vector3 position = orbitCenter.position + new Vector3(x, 0, z);

            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.SetParent(orbitCenter);
            spawnedObjects.Add(obj); // guardar referencia
        }
    }

    void Update()
    {
        if (orbitCenter != null && target != null)
        {
            orbitCenter.position = target.position;
            orbitCenter.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (activateScripts)
        {
            ActivateAllScripts();
            activateScripts = false; 
        }
    }

    void ActivateAllScripts()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = true;
            }
        }
    }
}
