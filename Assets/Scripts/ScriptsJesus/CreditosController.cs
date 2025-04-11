using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con los botones
using UnityEngine.SceneManagement;

public class CreditosController : MonoBehaviour
{
    // Variable p�blica para el nombre de la escena del men�
    public string nombreEscenaMenu = "MenuPrincipalJesus";       // Puedes cambiarlo en el Inspector


    // Referencia p�blica al bot�n en el Inspector
    public Button volverButton;

    void Start()
    {
        // Verificar que el bot�n est� asignado desde el Inspector
        if (volverButton != null)
        {
            // Asignamos la funci�n que se ejecutar� cuando se haga clic en el bot�n
            volverButton.onClick.AddListener(VolverAlMenu);
        }
        else
        {
            Debug.LogError("�El bot�n no ha sido asignado en el Inspector!");
        }
    }


    // M�todo para volver al men� principal
    public void VolverAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu); // Carga la escena del men�
    }
}
