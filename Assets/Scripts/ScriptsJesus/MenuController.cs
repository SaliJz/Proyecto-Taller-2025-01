using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con los botones
using UnityEngine.SceneManagement;  // Necesario para cargar escenas

public class MenuController : MonoBehaviour
{
    // Variables públicas para asignar los botones desde el Inspector
    public Button jugarButton;
    public Button creditosButton;
    public Button salirButton;
    public Button opcionesButton;

    // Variables para los nombres de las escenas, se pueden cambiar desde el Inspector
    public string nombreEscenaJuego = "Jesus";
    public string nombreEscenaCreditos = "Creditos";
    public string nombreEscenaOpciones = "MenuOpciones";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;  // Desbloquear el cursor al iniciar el juego
        Cursor.visible = true;  // Hacer visible el cursor

        // Asignar listeners a los botones, sin necesidad de usar OnClick() en el Inspector
        jugarButton.onClick.AddListener(StartGame);
        creditosButton.onClick.AddListener(ShowCredits);
        salirButton.onClick.AddListener(QuitGame);
        
        if (opcionesButton != null)
        {
            opcionesButton.onClick.AddListener(ShowOpciones);
        }
    }

    // Método para iniciar el juego
    public void StartGame()
    {

        SceneManager.LoadScene(nombreEscenaJuego);  // Carga la escena especificada
    }

    // Método para salir del juego
    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();  // Cierra la aplicación
    }

    // Método para mostrar los créditos
    public void ShowCredits()
    {
        SceneManager.LoadScene(nombreEscenaCreditos);  
    }

    public void ShowOpciones()
    {
        SceneManager.LoadScene(nombreEscenaOpciones);  
    }
}
