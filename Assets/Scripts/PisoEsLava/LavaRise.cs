using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaRise : MonoBehaviour
{
    public float riseSpeed = 1f; 
    public float maxHeight = 10f; 
    private bool isActive = false;

    void Update()
    {
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
