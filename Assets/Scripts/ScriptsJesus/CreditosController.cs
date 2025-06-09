using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con los botones
using UnityEngine.SceneManagement;

public class CreditosController : MonoBehaviour
{
    public string nombreEscenaMenu = "MenuPrincipalJesus";
    public Button volverButton;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (volverButton != null)
        {
            volverButton.onClick.AddListener(VolverAlMenu);
        }
        else
        {
            Debug.LogError("¡El botón no ha sido asignado en el Inspector!");
        }
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}
