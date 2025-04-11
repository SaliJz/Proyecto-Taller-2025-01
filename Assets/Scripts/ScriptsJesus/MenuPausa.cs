using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    public GameObject canvasPausa;
    public Button botonContinuar;
    public Button botonMenuPrincipal;

    private bool juegoPausado = false;


    void Start()
    {
        // Asegurarse que el menú esté oculto al inicio
        canvasPausa.SetActive(false);

        // Asignar funciones a los botones por código
        botonContinuar.onClick.AddListener(ReanudarJuego);
        botonMenuPrincipal.onClick.AddListener(SalirAlMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESCAPE PRESIONADO");

            if (!juegoPausado)
                PausarJuego();
            else
                ReanudarJuego();
        }
    }

    public void PausarJuego()
    {
        Time.timeScale = 0f; // Detener el tiempo
        canvasPausa.SetActive(true);
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f; // Reanudar el tiempo
        canvasPausa.SetActive(false);
        juegoPausado = false;
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f; // Asegúrate de reanudar el tiempo antes de salir
        SceneManager.LoadScene("MenuPrincipalJesus");
    }
}
