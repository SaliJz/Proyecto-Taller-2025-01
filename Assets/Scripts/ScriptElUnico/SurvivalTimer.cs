using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SurvivalTimer : MonoBehaviour
{
    public SafeZone zone;
    public float currentTime = 0f;
    public float maxTime = 120f;
    public TMP_Text timerText;

    [Header("Progreso de Captura")]
    public Image progressBarFill;
    private float progress = 0f;
    public float captureSpeed = 0.1f;

    private Color green = Color.green;
    private Color red = Color.red;

    private bool capturing = false;

    void Update()
    {
        if (zone == null || progressBarFill == null || timerText == null)
        {
            Debug.LogWarning("Falta asignar referencias en el inspector");
            return;
        }

        if (zone.playerInside && zone.enemiesInside == 0)
        {
            capturing = true;
            progress += captureSpeed * Time.deltaTime;
            currentTime += Time.deltaTime;
        }
        else
        {
            capturing = false;
            if (!zone.playerInside)
                currentTime -= Time.deltaTime;
        }

        progress = Mathf.Clamp01(progress);
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        progressBarFill.fillAmount = progress;
        Color targetColor = capturing ? green : red;
        progressBarFill.color = Color.Lerp(progressBarFill.color, targetColor, Time.deltaTime * 4f);

        if (progress >= 1f)
        {
            Debug.Log("¡Ganaste!");
            GameManager.instance?.Win();
        }

        if (currentTime <= 0f)
        {
            Debug.Log("¡Perdiste!");
            GameManager.instance?.Lose();
        }

        Debug.Log($"Capturando: {capturing} | Progreso: {progress} | Tiempo: {currentTime}");
    }  
}
