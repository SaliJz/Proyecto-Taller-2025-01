using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class VolumenController : MonoBehaviour
{
    public Slider volumenSlider;
    public Button volverButton;

    void Start()
    {
        // Cargar volumen previamente guardado (si lo hay)
        float volumenGuardado = PlayerPrefs.GetFloat("volumenGeneral", 1f);
        volumenSlider.value = volumenGuardado;
        AudioListener.volume = volumenGuardado;

        // Listeners
        volumenSlider.onValueChanged.AddListener(CambiarVolumen);
        volverButton.onClick.AddListener(VolverAlMenuOpciones);
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("volumenGeneral", valor); // Guarda para siguiente vez
    }

    public void VolverAlMenuOpciones()
    {
        SceneManager.LoadScene("MenuOpciones");
    }
}
