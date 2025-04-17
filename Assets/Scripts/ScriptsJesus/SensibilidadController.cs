using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class SensibilidadController : MonoBehaviour
{
    public Slider sensibilidadSlider;
    public Button volverButton;
    public TextMeshProUGUI porcentajeTexto;

    void Start()
    {
        // Cargar sensibilidad previa (si existe), por defecto 0.5
        float sensibilidadGuardada = PlayerPrefs.GetFloat("sensibilidad", 0.5f);
        sensibilidadSlider.value = sensibilidadGuardada;
        ActualizarTexto(sensibilidadGuardada);

        // Listeners
        sensibilidadSlider.onValueChanged.AddListener(CambiarSensibilidad);
        volverButton.onClick.AddListener(VolverAlMenuOpciones);
    }


    public void CambiarSensibilidad(float valor)
    {
        PlayerPrefs.SetFloat("sensibilidad", valor); // Guarda el valor para usarlo en otras escenas
        ActualizarTexto(valor);
    }


    private void ActualizarTexto(float valor)
    {
        int porcentaje = Mathf.RoundToInt(valor * 100f);
        if (porcentajeTexto != null)
            porcentajeTexto.text = "Sensibilidad: " + porcentaje + "%";
    }

    public void VolverAlMenuOpciones()
    {
        SceneManager.LoadScene("MenuOpciones");
    }

}
