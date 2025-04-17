using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResolucionesController : MonoBehaviour
{
    public TMP_Dropdown resolucionDropdown;
    public Button volverButton;

    private Resolution[] resolucionesDisponibles;

     void Start()
    {
        // Obtener las resoluciones soportadas por el monitor
        resolucionesDisponibles = Screen.resolutions;
        resolucionDropdown.ClearOptions();

        List<string> opciones = new List<string>();
        int resolucionActualIndex = 0;

        for (int i = 0; i < resolucionesDisponibles.Length; i++)
        {
            string opcion = resolucionesDisponibles[i].width + " x " + resolucionesDisponibles[i].height;
            opciones.Add(opcion);

            if (resolucionesDisponibles[i].width == Screen.currentResolution.width &&
                resolucionesDisponibles[i].height == Screen.currentResolution.height)
            {
                resolucionActualIndex = i;
            }
        }

        resolucionDropdown.AddOptions(opciones);
        resolucionDropdown.value = resolucionActualIndex;
        resolucionDropdown.RefreshShownValue();

        // Agregar listener al cambio de valor en el dropdown
        resolucionDropdown.onValueChanged.AddListener(CambiarResolucion);
        volverButton.onClick.AddListener(VolverAlMenuOpciones);
    }

    public void CambiarResolucion(int indice)
    {
        Resolution resolucion = resolucionesDisponibles[indice];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
    }

    public void VolverAlMenuOpciones()
    {
        SceneManager.LoadScene("MenuOpciones"); // Asegúrate que se llame así tu escena
    }

}
