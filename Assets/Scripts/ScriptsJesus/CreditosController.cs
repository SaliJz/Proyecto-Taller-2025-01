using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con los botones
using UnityEngine.SceneManagement;

public class CreditosController : MonoBehaviour
{
    // Variable pública para el nombre de la escena del menú
    public string nombreEscenaMenu = "MenuPrincipalJesus";       // Puedes cambiarlo en el Inspector


    // Referencia pública al botón en el Inspector
    public Button volverButton;

    void Start()
    {
        // Verificar que el botón esté asignado desde el Inspector
        if (volverButton != null)
        {
            // Asignamos la función que se ejecutará cuando se haga clic en el botón
            volverButton.onClick.AddListener(VolverAlMenu);
        }
        else
        {
            Debug.LogError("¡El botón no ha sido asignado en el Inspector!");
        }
    }


    // Método para volver al menú principal
    public void VolverAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu); // Carga la escena del menú
    }
}
