// VidaEnemigoGeneral.cs
using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

    [Header("Configuración de enemigo")]
    public TipoEnemigo tipo;                // Se sobreescribe en Start()
    public float vida = 100f;               // Vida total
    public Slider sliderVida;               // Slider UI (opcional)

    [Header("Daño por balas")]
    public float danioCoincidente = 20f;    // Ahora es el daño ALTO (cuando NO coincide)
    public float danioNoCoincidente = 5f;   // Ahora es el daño BAJO (cuando SÍ coincide)

    void Start()
    {
        // 1) Elegir al azar uno de los 3 tipos de enemigo
        tipo = (TipoEnemigo)Random.Range(0, 3);
        Debug.Log($"[VidaEnemigo] Tipo asignado: {tipo}");

        // 2) Pintar al enemigo del color asociado al tipo
        Color colorAsignado;
        switch (tipo)
        {
            case TipoEnemigo.Ametralladora:
                colorAsignado = Color.blue;
                break;
            case TipoEnemigo.Pistola:
                colorAsignado = Color.red;
                break;
            case TipoEnemigo.Escopeta:
                colorAsignado = Color.green;
                break;
            default:
                colorAsignado = Color.white;
                break;
        }

        // Si es un objeto 3D con Renderer
        Renderer rend3D = GetComponent<Renderer>();
        if (rend3D != null)
        {
            rend3D.material.color = colorAsignado;
        }
        else
        {
            // Si es un sprite 2D
            SpriteRenderer rend2D = GetComponent<SpriteRenderer>();
            if (rend2D != null)
                rend2D.color = colorAsignado;
        }

        // 3) Inicializar slider de vida
        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

    // Reduce vida y actualiza slider
    public void RecibirDanio(float danio)
    {
        vida -= danio;
        if (sliderVida != null)
            sliderVida.value = vida;

        if (vida <= 0f)
        {
            vida = 0f;
            Morir();
        }
    }

    // Llamar desde la bala, pasándole su tag
    public void RecibirDanioPorBala(string tagBala)
    {
        // Por defecto daño ALTO (cuando NO coincide)
        float danioAAplicar = danioCoincidente;

        // Si el tag coincide con el tipo de enemigo, aplicamos el daño BAJO
        if ((tipo == TipoEnemigo.Ametralladora && tagBala == "BalaAmetralladora") ||
            (tipo == TipoEnemigo.Pistola && tagBala == "BalaPistola") ||
            (tipo == TipoEnemigo.Escopeta && tagBala == "BalaEscopeta"))
        {
            danioAAplicar = danioNoCoincidente;
        }

        RecibirDanio(danioAAplicar);
    }

    void Morir()
    {
        // Aquí puedes reproducir partículas, sonidos, etc.
        Destroy(gameObject);
    }
}
