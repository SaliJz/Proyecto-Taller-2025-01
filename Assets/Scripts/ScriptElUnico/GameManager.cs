using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración Temporizador")]
    public float totalTime = 120f;
    public Slider timerSlider;
    public TMP_Text timerText;

    [Header("Captura de Zona")]
    public Slider captureProgressSlider;
    public TMP_Text captureStatusText;

    [Header("Zona segura")]
    public SafeZone safeZone;

    [Header("Color de barra")]
    public Color colorJugadorSolo = Color.green;
    public Color colorConEnemigos = Color.red;

    private float currentTime;
    private bool timerRunning = false;
    private bool zonaActivo = false;

    private bool jugadorEnZona = false;
    private bool enemigosEnZona = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActivateRandomZone();

       
        StartTimer();

        currentTime = totalTime;
        timerSlider.maxValue = totalTime;
        timerSlider.value = totalTime;
        captureProgressSlider.value = 0;
        captureProgressSlider.maxValue = 1;
        captureProgressSlider.gameObject.SetActive(false);
        captureStatusText.text = "";

        safeZone.OnPlayerStateChange += HandlePlayerStateChange;

        UpdateCaptureBarColor(); 
    }

    void ActivateRandomZone()
    {
        safeZone.gameObject.SetActive(true);
        zonaActivo = true;
        Debug.Log("Zona activada");
    }

    
    public void StartTimer()
    {
        currentTime = totalTime;
        timerRunning = true;
    }

    void HandlePlayerStateChange(bool entered)
    {
        jugadorEnZona = entered;

        if (entered)
        {
            captureProgressSlider.gameObject.SetActive(true);
            captureStatusText.text = "Capturando...";
  
            if (captureProgressSlider != null)
                captureProgressSlider.fillRect.GetComponent<Image>().color = colorJugadorSolo;
        }
        else
        {
            if (captureProgressSlider != null)
                captureProgressSlider.fillRect.GetComponent<Image>().color = colorConEnemigos;
            captureProgressSlider.value = 0;
            captureStatusText.text = "";
        }

        UpdateCaptureBarColor();
    }

    void Update()
    {
        if (!zonaActivo) return;

        enemigosEnZona = safeZone.enemiesInside > 0;

        UpdateCaptureBarColor();

        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;
            timerSlider.value = currentTime;
            UpdateTimerText();

            if (currentTime <= 0)
            {
                if (captureProgressSlider.value < 1f)
                {
                    Debug.Log("¡Se acabó el tiempo! Perdiste");
                    GameOver(false);
                }
                else
                {
                    Debug.Log("¡Zona capturada! Ganaste");
                    GameOver(true);
                }
            }
        }

        if (jugadorEnZona && !enemigosEnZona && captureProgressSlider.gameObject.activeSelf)
        {
            captureProgressSlider.value += Time.deltaTime;
            if (captureProgressSlider.value >= 1f)
            {
                captureProgressSlider.value = 1;
                Debug.Log("¡Zona capturada! Ganaste");
                GameOver(true);
            }
        }
        
    }

    void UpdateTimerText()
    {
        int min = Mathf.FloorToInt(currentTime / 60f);
        int sec = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    void UpdateCaptureBar()
    {
        // La lógica ya está en Update()
    }

    void UpdateCaptureBarColor()
    {
        if (captureProgressSlider != null)
        {
            if (jugadorEnZona && !enemigosEnZona)
            {
                
                captureProgressSlider.fillRect.GetComponent<Image>().color = colorJugadorSolo;
            }
            else
            {
                captureProgressSlider.fillRect.GetComponent<Image>().color = colorConEnemigos;
            }
        }
    }

    void GameOver(bool win)
    {
        timerRunning = false; 
        captureStatusText.text = win ? "¡Ganaste!" : "¡Perdiste!";
    }




}
