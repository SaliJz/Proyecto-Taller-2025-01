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

    private float timeSinceStart = 0f; 
    private bool missionStarted = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActivateRandomZone();

        currentTime = 0f;
        timerSlider.maxValue = totalTime;
        timerSlider.value = 0f;

        UpdateTimerText();

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


    void HandlePlayerStateChange(bool entered)
    {
        jugadorEnZona = entered;

        if (entered)
        {
            captureProgressSlider.gameObject.SetActive(true);
            captureStatusText.text = "Capturando...";
            captureProgressSlider.fillRect.GetComponent<Image>().color = colorJugadorSolo;

            if (!timerRunning)
                StartTimer();

        }
        else
        {
            captureProgressSlider.fillRect.GetComponent<Image>().color = colorConEnemigos;
            captureStatusText.text = "¡Fuera de la zona!";
        }

        UpdateCaptureBarColor();
    }

    void StartTimer()
    {
        timerRunning = true;
        missionStarted = true;
    }

    void Update()
    {
        if (!zonaActivo) return;

        enemigosEnZona = safeZone.enemiesInside > 0;

        UpdateCaptureBarColor();

        if (!missionStarted)
        {
            timeSinceStart += Time.deltaTime;

            if (timeSinceStart >= 30f && !jugadorEnZona)
            {
                Debug.Log("No entraste a tiempo a la zona. Perdiste.");
                GameOver(false);
                return;
            }
        }

        if (timerRunning)
        {
            if (jugadorEnZona && !enemigosEnZona)
            {
                currentTime += Time.deltaTime;
                if (currentTime > totalTime) currentTime = totalTime;
            }
            else if (!jugadorEnZona)
            {
                currentTime -= Time.deltaTime;
                if (currentTime < 0) currentTime = 0;
            }

            timerSlider.value = currentTime;
            UpdateTimerText();

            if (currentTime >= totalTime)
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

        if (jugadorEnZona)
        {
            if (!enemigosEnZona)
            {
                captureProgressSlider.value += Time.deltaTime / totalTime;

                if (captureProgressSlider.value >= 1f)
                {
                    captureProgressSlider.value = 1f;
                    Debug.Log("¡Zona capturada! Ganaste");
                    GameOver(true);
                }
            }
            else
            {
                captureStatusText.text = "¡Enemigos detectados!";
            }
        }

        else
        {
            if (captureProgressSlider.value > 0)
            {
                captureProgressSlider.value -= Time.deltaTime / (totalTime / 2f); 
                if (captureProgressSlider.value < 0) captureProgressSlider.value = 0;
            }
        }
    }

    void UpdateTimerText()
    {
        int min = Mathf.FloorToInt(currentTime / 60f);
        int sec = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{min:00}:{sec:00}";
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
        Debug.Log(win ? "VICTORIA" : "DERROTA");
    }
}
