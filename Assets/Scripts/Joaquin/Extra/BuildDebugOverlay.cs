using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BuildDebugOverlay : MonoBehaviour
{
    private float fps;
    private float timer;
    private string debugLog = "";

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= 1f)
        {
            fps = 1f / Time.unscaledDeltaTime;
            timer = 0f;
        }

        GameObject player = GameObject.FindWithTag("Player");
        GameObject cam = Camera.main != null ? Camera.main.gameObject : null;
        GameObject hud = GameObject.Find("HUD") ?? GameObject.Find("CanvasHUD");

        EventSystem eventSystem = EventSystem.current;

        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        int audioPlaying = 0;
        foreach (var a in audios) if (a.isPlaying) audioPlaying++;

        debugLog =
            $"DEBUG BUILD\n" +
            $"Scene: {SceneManager.GetActiveScene().name}\n" +
            $"FPS: {Mathf.RoundToInt(fps)}\n" +
            $"Time.timeScale: {Time.timeScale}\n" +
            $"Player encontrado: {(player != null ? "Si" : "No")}\n" +
            $"Cámara principal: {(cam != null ? "Si" : "No")}\n" +
            $"HUD encontrado: {(hud != null ? "Si" : "No")}\n" +
            $"EventSystem: {(eventSystem != null ? "Si" : "No")}\n" +
            $"AudioSources: {audios.Length} ({audioPlaying} activos)\n";
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        GUI.Box(new Rect(10, 10, 400, 200), debugLog, style);
    }
}