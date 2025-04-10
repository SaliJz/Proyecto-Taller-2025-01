using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    // Vida total del enemigo
    public float vida = 100f;

    // Referencia al Slider que muestra la vida
    public Slider sliderVida;

    // Se inicializa la barra de vida
    void Start()
    {
        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

    // M�todo p�blico que permite recibir da�o
    public void RecibirDanio(float danio)
    {
        vida -= danio;

        // Actualiza la barra de vida en el slider
        if (sliderVida != null)
        {
            sliderVida.value = vida;
        }

        // Si la vida es menor o igual a cero, ejecuta el proceso de "muerte"
        if (vida <= 0)
        {
            vida = 0;
            Morir();
        }
    }

    // M�todo que maneja la "muerte" del enemigo
    void Morir()
    {
        // Aqu� puedes agregar efectos visuales, part�culas o sonidos
        Destroy(gameObject);
    }
}
