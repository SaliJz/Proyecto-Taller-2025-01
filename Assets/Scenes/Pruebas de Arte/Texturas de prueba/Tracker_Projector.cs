using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Tracker_Projector : MonoBehaviour
{
    [SerializeField] Transform MarkerProjected;
    [SerializeField] float Rotation_Speed;


    private void Update()
    {
        if (MarkerProjected != null) 
        {
            transform.rotation = Quaternion.LookRotation(MarkerProjected.position - transform.position);

            float y = MarkerProjected.eulerAngles.y + Rotation_Speed * Time.deltaTime;
            MarkerProjected.rotation = Quaternion.Euler(90, y, 0);
        }
    }
}
