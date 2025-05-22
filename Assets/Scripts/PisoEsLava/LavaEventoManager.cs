using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaEventoManager : MonoBehaviour
{
    public GameObject lava;
    public GameObject[] platforms; 
    public float countdown = 10f; 
    public float ascentInterval = 30f; 
    public float ascentSpeed = 0.5f;

    private float timer;
    private float ascentTimer;

    void Start()
    {
        timer = countdown;
        ascentTimer = ascentInterval;
        
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            ascentTimer -= Time.deltaTime;
            if (ascentTimer <= 0)
            {
                StartCoroutine(SubirLava());
                ascentTimer = ascentInterval;
            }
        }
    }


    IEnumerator SubirLava()
    {
        float targetY = lava.transform.position.y + 1f; 
        while (lava.transform.position.y < targetY)
        {
            lava.transform.position += Vector3.up * ascentSpeed * Time.deltaTime;
            yield return null;
        }
        Debug.Log("La lava ha subido");
    }


}
