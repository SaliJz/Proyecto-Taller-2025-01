using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaSpawner : MonoBehaviour
{
    public GameObject[] plataformas; 
    public float tiempoEntrePlataformas = 2f;
    public float alturaIncremento = 2f;
    public float rangoX = 5f;

    private float alturaActual;

    void Start()
    {
        alturaActual = transform.position.y;
        StartCoroutine(SpawnPlataformas());
    }

    IEnumerator SpawnPlataformas()
    {
        while (true)
        {
            SpawnUnaPlataforma();
            yield return new WaitForSeconds(tiempoEntrePlataformas);
        }
    }

    void SpawnUnaPlataforma()
    {
        float posX = Random.Range(-rangoX, rangoX);
        GameObject plataformaElegida = plataformas[Random.Range(0, plataformas.Length)];

        Vector3 spawnPos = new Vector3(posX, alturaActual, transform.position.z);
        Instantiate(plataformaElegida, spawnPos, Quaternion.identity);

        alturaActual += alturaIncremento;
    }

    public void ActivarEvento()
    {
        StartCoroutine(SpawnPlataformas());
    }

}
