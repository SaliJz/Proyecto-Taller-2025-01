using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierDoor : MonoBehaviour
{
    private GenericDoor genericDoor;

    private void Start()
    {
        genericDoor = GetComponentInParent<GenericDoor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            genericDoor.PlayerPassedManualDoor();
        }
    }
}
