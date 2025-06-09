using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private float deltaTime;

    void Start()
    {
        if (fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (!fpsText.gameObject.activeSelf) return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fpsText.text = $"FPS: {Mathf.CeilToInt(1f / deltaTime)}";
    }
}
