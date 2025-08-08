using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWall : MonoBehaviour, IBurnable
{
    [SerializeField] private GameObject meltEffectPrefab;

    public void ApplyBurn(float duration)
    {
        Debug.Log("¡La pared de hielo se está derritiendo!");
        if (meltEffectPrefab != null)
        {
            Instantiate(meltEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Prefab de efecto de derretimiento no asignado.");
        }
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.25f);
    }
}