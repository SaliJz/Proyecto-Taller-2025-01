using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // Necesario para cargar escenas

public class MenuController : MonoBehaviour
{
    // Este método se llama cuando el jugador presiona "Inicio"
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");  // Cambia "GameScene" al nombre de tu escena de juego
    }

    // Este método se llama cuando el jugador presiona "Salir"
    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();  // Cierra el juego
    }

    public void ShowCredits()
    {
        SceneManager.LoadScene("Creditos");  // Cambia esto al nombre exacto de tu escena de créditos
    }
}
