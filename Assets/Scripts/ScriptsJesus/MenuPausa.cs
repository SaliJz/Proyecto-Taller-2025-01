using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject canvasPausa;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonMenuPrincipal;
    [SerializeField] private string nombreEscenaMenuPrincipal;
    [SerializeField] private Button botonSalir;

    private bool isDead = false; 
    private bool juegoPausado = false;

    void Start()
    {
        canvasPausa.SetActive(false);
       
        botonContinuar.onClick.AddListener(ReanudarJuego);
        botonMenuPrincipal.onClick.AddListener(SalirAlMenu);
        botonSalir.onClick.AddListener(SalirJuego);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isDead)
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
        Debug.Log("Juego pausado");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // Ocultar el cursor

        Time.timeScale = 0f;
        canvasPausa.SetActive(true);
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        Debug.Log("Juego reanudado");

        Cursor.lockState = CursorLockMode.Locked; // Desbloquear el cursor
        Cursor.visible = false; // Mostrar el cursor

        Time.timeScale = 1f; 
        canvasPausa.SetActive(false);
        juegoPausado = false;
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    public void SalirJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }

    public void SetIsDead(bool isDead)
    {
        this.isDead = isDead; // Actualizar el estado de muerte
    }
}
