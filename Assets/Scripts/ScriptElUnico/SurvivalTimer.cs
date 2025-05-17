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
    public float captureSpeed = 0.5f;
    public float decaySpeed = 0.3f;

    private Color green = new Color(0.2f, 0.8f, 0.2f);
    private Color red = new Color(0.8f, 0.2f, 0.2f);

    private bool wasCapturing = false;


    void Update()
    {
        if (zone == null || progressBarFill == null || timerText == null)
        {
            Debug.LogWarning("Falta asignar referencias en el inspector");
            return;
        }

        bool isCapturing = zone.playerInside && zone.enemiesInside == 0;

        if (isCapturing)
        {
            progress += captureSpeed * Time.deltaTime;
            currentTime += Time.deltaTime;
        }
        else
        {
            progress -= decaySpeed * Time.deltaTime;
            if (!zone.playerInside)
            {
                currentTime -= Time.deltaTime;
            }
        }

        progress = Mathf.Clamp01(progress);
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        UpdateTimerDisplay();
        UpdateProgressBar(isCapturing);

        if (progress >= 1f)
        {
            Debug.Log("¡Ganaste!");
            GameManager.instance?.Win();
            enabled = false; 
        }

        if (currentTime <= 0f)
        {
            Debug.Log("¡Perdiste!");
            GameManager.instance?.Lose();
            enabled = false; // Desactivar el script
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void UpdateProgressBar(bool isCapturing)
    {
        progressBarFill.fillAmount = progress;
        progressBarFill.color = isCapturing ? green : red;
    }

}
