using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con los botones
using UnityEngine.SceneManagement;  // Necesario para cargar escenas

public class MenuController : MonoBehaviour
{
    // Variables p�blicas para asignar los botones desde el Inspector
    public Button jugarButton;
    public Button creditosButton;
    public Button salirButton;

    // Variables para los nombres de las escenas, se pueden cambiar desde el Inspector
    public string nombreEscenaJuego = "Jesus";
    public string nombreEscenaCreditos = "CreditosJesus";

    void Start()
    {
        // Asignar listeners a los botones, sin necesidad de usar OnClick() en el Inspector
        jugarButton.onClick.AddListener(StartGame);
        creditosButton.onClick.AddListener(ShowCredits);
        salirButton.onClick.AddListener(QuitGame);
    }


    // M�todo para iniciar el juego
    public void StartGame()
    {
        SceneManager.LoadScene(nombreEscenaJuego);  // Carga la escena especificada
    }

    // M�todo para salir del juego
    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();  // Cierra la aplicaci�n
    }

    // M�todo para mostrar los cr�ditos
    public void ShowCredits()
    {
        SceneManager.LoadScene(nombreEscenaCreditos);  // Carga la escena de cr�ditos
    }
}
