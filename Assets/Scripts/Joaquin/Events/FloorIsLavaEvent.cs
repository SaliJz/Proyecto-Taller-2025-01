using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorIsLavaEvent : MonoBehaviour
{
    [Header("Lava Settings")]
    [SerializeField] private GameObject lava;
    [SerializeField] private float ascentSpeed = 0.5f;
    [SerializeField] private float ascentHeight = 1f;
    [SerializeField] private float maxLavaHeight = 10f;

    [Header("Timing")]
    [SerializeField] private float countdown = 10f;
    [SerializeField] private float ascentInterval = 30f;

    [Header("Platforms")]
    [SerializeField] private GameObject[] platforms;

    private float timer;
    private float ascentTimer;
    private bool isActive = false;

    private Transform lavaOriginalTransform;

    private void OnEnable()
    {
        if (lava == null)
        {
            Debug.LogError("[FloorIsLavaEvent] Lava no está asignada.");
            return;
        }

        lavaOriginalTransform = lava.transform;
        lava.transform.position = lavaOriginalTransform.position;

        ActivateEvent();
    }

    private void OnDisable()
    {
        DeactivateEvent();
    }

    public void ActivateEvent()
    {
        isActive = true;
        timer = countdown;
        ascentTimer = ascentInterval;

        if (platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("[FloorIsLavaEvent] No plataformas asignadas");
            return;
        }

        foreach (GameObject platform in platforms)
        {
            platform.SetActive(true);
        }
    }

    public void DeactivateEvent()
    {
        isActive = false;

        if (platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("[FloorIsLavaEvent] No plataformas asignadas");
            return;
        }

        foreach (GameObject platform in platforms)
        {
            platform.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isActive) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            ascentTimer -= Time.deltaTime;
            if (ascentTimer <= 0)
            {
                StartCoroutine(RaiseLava());
                ascentTimer = ascentInterval;
            }
        }
    }

    IEnumerator RaiseLava()
    {
        float currentY = lava.transform.position.y;
        float targetY = Mathf.Min(currentY + ascentHeight, maxLavaHeight);
        while (lava.transform.position.y < targetY)
        {
            float step = ascentSpeed * Time.deltaTime;
            float newY = Mathf.Min(lava.transform.position.y + step, targetY);
            lava.transform.position = new Vector3(
                lava.transform.position.x,
                newY,
                lava.transform.position.z
            );
            yield return null;
        }
    }
}
