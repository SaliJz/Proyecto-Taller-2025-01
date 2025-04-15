using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class NombreJugador: MonoBehaviour
{
    public TMP_InputField inputNombre; // Campo para escribir el nombre
    public Button botonComenzar;       // Botón para comenzar el juego

    public static string nombreJugador; // Variable accesible desde cualquier escena

    void Start()
    {
        botonComenzar.onClick.AddListener(ComenzarJuego);
    }

    void ComenzarJuego()
    {
        nombreJugador = inputNombre.text;
        Debug.Log("Nombre del jugador: " + nombreJugador);

        SceneManager.LoadScene("Jesus"); 
    }

}
