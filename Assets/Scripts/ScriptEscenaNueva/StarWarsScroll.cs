using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarWarsScroll : MonoBehaviour
{
    public float speed = 50f; 

    private void Start()
    {
        
        transform.rotation = Quaternion.Euler(25f, 0f, 0f); 
    }

    private void Update()
    {

        transform.position += new Vector3(0, 1, -1) * speed * Time.deltaTime;
    }
}
