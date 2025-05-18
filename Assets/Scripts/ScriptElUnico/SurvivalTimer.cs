using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Referencias Obligatorias")]
    public SafeZone zone;
    [SerializeField] private Image progressBarFill;
    public TMP_Text timerText;

    [Header("Configuración")]
    public float maxTime = 120f;
    public float captureSpeed = 1f;
    public float decaySpeed = 0.5f;

    private float currentTime;
    private float progress = 0f;
    private bool shouldCountTime = true;
    private bool isPaused = false;



    void Start()
    {
        currentTime = maxTime;
        ResetProgressBar();
        UpdateUI(false);
        if (zone != null)
        {
            zone.OnPlayerStateChange += HandlePlayerStateChange;
        }

    }
    private void HandlePlayerStateChange(bool entered)
    {
        if (entered)
        {
            isPaused = true;
            currentTime = maxTime; 
            progress = 1f; 
            UpdateProgress();
            UpdateUI(true);
        }
        else
        {
            
            isPaused = false;
            currentTime = maxTime; 
            progress = 0f; 
            UpdateProgress();
            UpdateUI(false);
        }
    }

    void Update()
    {
        if (!CheckReferences()) return;

       
        bool isPlayerSafe = zone.playerInside && zone.enemiesInside == 0;

        if (isPlayerSafe)
        {
            
            progress = 1f;
            UpdateProgress();
            UpdateUI(true);
            return; 
        }

        if (isPlayerSafe)
        {
            progress = 1f;
            UpdateProgress();
            UpdateUI(true);
            return; 
        }

        if (shouldCountTime)
        {
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(currentTime, 0f);
        }

        progress = 1f - (currentTime / maxTime);
        UpdateProgress();

        UpdateUI(false);
        CheckGameOver();
    }

    void ResetProgressBar()
    {
        progress = 0f;
        if (progressBarFill != null)
        {
            if (progressBarFill.type == Image.Type.Filled)
            {
                progressBarFill.fillAmount = 0f; 
            }
            progressBarFill.color = Color.red;
        }
    }

    void UpdateProgress()
    {
        progress = Mathf.Clamp01(progress);
        if (progressBarFill != null)
        {
            var slider = progressBarFill.GetComponent<Slider>();
            if (slider != null)
            {
                slider.value = progress;
            }
            else
            {
                var image = progressBarFill.GetComponent<Image>();
                if (image != null && image.type == Image.Type.Filled)
                {
                    image.fillAmount = progress;
                }
            }
            progressBarFill.SetAllDirty();
        }
    }

    bool CheckReferences()
    {
        if (zone == null) Debug.LogError("¡No hay SafeZone asignado!");
        if (progressBarFill == null) Debug.LogError("¡Falta la barra de progreso!");
        if (timerText == null) Debug.LogError("¡Falta el texto del temporizador!");

        if (progressBarFill != null && progressBarFill.type != Image.Type.Filled)
        {
            Debug.LogError("La barra de progreso debe ser de tipo Filled");
            progressBarFill.type = Image.Type.Filled;
        }

        return zone != null && progressBarFill != null && timerText != null;
    }

    void UpdateUI(bool isPlayerSafe)
    {
        if (progressBarFill != null)
        {
            progressBarFill.color = isPlayerSafe ?
                new Color(0.2f, 0.8f, 0.2f) : 
                new Color(0.8f, 0.2f, 0.2f);   
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        Debug.Log($"Progreso: {progress:F2} | Fill: {progressBarFill?.fillAmount} | " +
                  $"Color: {(isPlayerSafe ? "VERDE" : "ROJO")} | " +
                  $"Tiempo: {currentTime:F2}");
    }

    void CheckGameOver()
    {
       
    }
}



   