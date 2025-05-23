using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformasMoviles : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    private Vector3 target;

    void Start()
    {
        transform.position = pointA.position;
        target = pointB.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
           
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }
    }

}
