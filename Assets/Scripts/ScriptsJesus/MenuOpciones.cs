using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuOpciones : MonoBehaviour
{
    public Button resolucionesButton;
    public Button volumenButton;
    public Button sensibilidadButton;
    public Button volverButton;

    // Nombres de las escenas a las que se irá
    public string nombreEscenaResoluciones = "Resoluciones";
    public string nombreEscenaVolumen = "Volumen";
    public string nombreEscenaSensibilidad = "Sensibilidad";
    public string nombreEscenaMenuPrincipal = "MenuPrincipalJesus"; // Asegúrate de que el nombre coincida con tu escena principal

    void Start()
    {
        resolucionesButton.onClick.AddListener(IrAResoluciones);
        volumenButton.onClick.AddListener(IrAVolumen);
        sensibilidadButton.onClick.AddListener(IrASensibilidad);
        volverButton.onClick.AddListener(VolverAlMenuPrincipal);
    }


    public void IrAResoluciones()
    {
        SceneManager.LoadScene(nombreEscenaResoluciones);
    }

    public void IrAVolumen()
    {
        SceneManager.LoadScene(nombreEscenaVolumen);
    }

    public void IrASensibilidad()
    {
        SceneManager.LoadScene(nombreEscenaSensibilidad);
    }

    public void VolverAlMenuPrincipal()
    {
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }






}
