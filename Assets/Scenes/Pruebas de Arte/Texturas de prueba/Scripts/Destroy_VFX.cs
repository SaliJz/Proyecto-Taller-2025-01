using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy_VFX : MonoBehaviour
{
    public float Destroy_Delay;
    void Start()
    {
        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(Destroy_Delay);

        Destroy(gameObject);
    }
}
