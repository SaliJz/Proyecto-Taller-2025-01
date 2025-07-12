using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class NombreJugador : MonoBehaviour
{
    public TMP_InputField inputNombre;
    public Button botonComenzar;

    public static string nombreJugador;

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
