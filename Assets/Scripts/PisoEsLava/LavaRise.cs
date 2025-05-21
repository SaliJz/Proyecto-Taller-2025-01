using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LavaRise : MonoBehaviour
{
    public float riseSpeed = 1f; 
    public float maxHeight = 10f;
    public float tiempoParaSubir = 10f; 
    public TextMeshProUGUI timerText;

    private float timer;
    private bool isActive = false;
    private bool lavaStarted = false;

    void Start()
    {
        timer = tiempoParaSubir;
    }


    void Update()
    {
         if (!lavaStarted)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = "¡La lava sube en: " + Mathf.CeilToInt(timer).ToString() + "s!";
            }

            if (timer <= 0f)
            {
                StartLava();
                lavaStarted = true;

                if (timerText != null)
                {
                    timerText.text = "¡La lava está subiendo!";
                }
            }
        }

        if (isActive && transform.position.y < maxHeight)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        }
    }

    public void StartLava()
    {
        isActive = true;
    }

    public void StopLava()
    {
        isActive = false;
    }
}
