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
        
        canvasPausa.SetActive(false);

       
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
        Time.timeScale = 0f; 
        canvasPausa.SetActive(true);
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f; 
        canvasPausa.SetActive(false);
        juegoPausado = false;
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MenuPrincipalJesus");
    }
}
