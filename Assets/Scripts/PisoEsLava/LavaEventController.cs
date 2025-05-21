using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaEventController : MonoBehaviour
{
    public LavaRise lava;
    public PlataformaSpawner spawner;

    public float tiempoParaIniciarEvento = 30f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tiempoParaIniciarEvento)
        {
            IniciarEvento();
            this.enabled = false; 
        }
    }

    void IniciarEvento()
    {
        lava.StartLava();
        spawner.ActivarEvento();
    }
}
