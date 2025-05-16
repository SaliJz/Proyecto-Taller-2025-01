using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    public SafeZone zone;
    public float currentTime = 0f;
    public float maxTime = 120f;
    public TMP_Text timerText;

    void Update()
    {
        if (zone.playerInside && zone.enemiesInside == 0)
        {
            currentTime += Time.deltaTime;
        }
        else if (!zone.playerInside)
        {
            currentTime -= Time.deltaTime;
        }

        currentTime = Mathf.Clamp(currentTime, 0, maxTime);

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (currentTime >= maxTime)
        {
            GameManager.instance.Win();
        }
    }


}
