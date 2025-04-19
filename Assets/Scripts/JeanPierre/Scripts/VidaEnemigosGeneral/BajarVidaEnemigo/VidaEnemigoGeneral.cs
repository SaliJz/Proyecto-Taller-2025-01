using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

    // Configuración del enemigo
    public TipoEnemigo tipo;           // Se asigna en Start()
    public float vida = 100f;          // Vida inicial
    public Slider sliderVida;          // Slider UI opcional

    // Daño por bala
    public float danioAlto = 20f;      // Cuando la bala NO coincide con el tipo
    public float danioBajo = 5f;       // Cuando la bala coincide con el tipo

    // Prefabs para instanciar al morir
    [Tooltip("Arrastra aquí los prefabs que pueden generarse al morir")]
    public GameObject[] prefabsAlMorir;

    void Start()
    {
        // Elegir tipo al azar
        tipo = (TipoEnemigo)Random.Range(0, 3);
        Debug.Log("[VidaEnemigo] Tipo asignado: " + tipo);

        // Pintar el color según el tipo
        Color colorAsignado;
        switch (tipo)
        {
            case TipoEnemigo.Ametralladora: colorAsignado = Color.blue; break;
            case TipoEnemigo.Pistola: colorAsignado = Color.red; break;
            case TipoEnemigo.Escopeta: colorAsignado = Color.green; break;
            default: colorAsignado = Color.white; break;
        }

        // Aplicar color (3D o 2D)
        Renderer rend3D = GetComponent<Renderer>();
        if (rend3D != null)
            rend3D.material.color = colorAsignado;
        else if (TryGetComponent<SpriteRenderer>(out var rend2D))
            rend2D.color = colorAsignado;

        // Inicializar slider
        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

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

    public void RecibirDanioPorBala(string tagBala)
    {
        // Por defecto aplicamos daño alto
        float danioAAplicar = danioAlto;

        // Si coincide bala <-> tipo, aplicamos daño bajo
        if ((tipo == TipoEnemigo.Ametralladora && tagBala == "BalaAmetralladora") ||
            (tipo == TipoEnemigo.Pistola && tagBala == "BalaPistola") ||
            (tipo == TipoEnemigo.Escopeta && tagBala == "BalaEscopeta"))
        {
            danioAAplicar = danioBajo;
        }

        RecibirDanio(danioAAplicar);
    }

    void Morir()
    {
        // Instanciar un prefab de los disponibles (si hay al menos uno)
        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
        {
            // Elegir uno al azar
            int idx = Random.Range(0, prefabsAlMorir.Length);
            GameObject prefabSeleccionado = prefabsAlMorir[idx];

            // Instanciar en la posición y rotación actuales
            Instantiate(prefabSeleccionado, transform.position, transform.rotation);
        }
        // Aquí puedes agregar partículas, sonido, etc.

        // Destruir el objeto enemigo
        Destroy(gameObject);
    }
}





//using UnityEngine;
//using UnityEngine.UI;

//public class VidaEnemigoGeneral : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

//    // Configuracion del enemigo
//    public TipoEnemigo tipo;      // Se asigna en Start()
//    public float vida = 100f;     // Vida inicial
//    public Slider sliderVida;     // Slider UI opcional

//    // Danio por bala
//    public float danioAlto = 20f; // Cuando la bala NO coincide con el tipo
//    public float danioBajo = 5f;  // Cuando la bala coincide con el tipo

//    void Start()
//    {
//        // Elegir tipo al azar
//        tipo = (TipoEnemigo)Random.Range(0, 3);
//        Debug.Log("[VidaEnemigo] Tipo asignado: " + tipo);

//        // Pintar el color segun el tipo
//        Color colorAsignado;
//        switch (tipo)
//        {
//            case TipoEnemigo.Ametralladora: colorAsignado = Color.blue; break;
//            case TipoEnemigo.Pistola: colorAsignado = Color.red; break;
//            case TipoEnemigo.Escopeta: colorAsignado = Color.green; break;
//            default: colorAsignado = Color.white; break;
//        }

//        Renderer rend3D = GetComponent<Renderer>();
//        if (rend3D != null)
//            rend3D.material.color = colorAsignado;
//        else
//        {
//            SpriteRenderer rend2D = GetComponent<SpriteRenderer>();
//            if (rend2D != null)
//                rend2D.color = colorAsignado;
//        }

//        // Inicializar slider
//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }
//    }

//    public void RecibirDanio(float danio)
//    {
//        vida -= danio;
//        if (sliderVida != null)
//            sliderVida.value = vida;

//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    public void RecibirDanioPorBala(string tagBala)
//    {
//        // Por defecto aplicamos danio alto
//        float danioAAplicar = danioAlto;

//        // Si coincide bala <-> tipo, aplicamos danio bajo
//        if ((tipo == TipoEnemigo.Ametralladora && tagBala == "BalaAmetralladora") ||
//            (tipo == TipoEnemigo.Pistola && tagBala == "BalaPistola") ||
//            (tipo == TipoEnemigo.Escopeta && tagBala == "BalaEscopeta"))
//        {
//            danioAAplicar = danioBajo;
//        }

//        RecibirDanio(danioAAplicar);
//    }

//    void Morir()
//    {
//        // Aqui puedes poner particulas, sonido, etc.
//        Destroy(gameObject);
//    }
//}

