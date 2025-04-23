using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class TutorialManager : MonoBehaviour
{
    public TMP_Text tutorialText;

    private int currentStep = 0;


    private string[] tutorialMessages = new string[]
   {
        "Usa W, A, S, D para moverte",
        "Presiona ESPACIO para saltar",
        "Presiona LEFT SHIFT para hacer Dash",
        "Mueve el mouse para controlar la cámara",
        "Haz CLICK IZQUIERDO para disparar",
        "Presiona R para recargar",
        "Presiona 1, 2 o 3 para cambiar de arma",
        "Haz CLICK DERECHO para usar tu habilidad",
        "Presiona Q para cambiar de habilidad",
        "Presiona ESC para pausar el juego",
        "Presiona E para interactuar con objetos"
   };

    void Start()
    {
        tutorialText.text = tutorialMessages[currentStep];
    }

    
    void Update()
    {
        if (currentStep == 0 && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                                 Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
            NextStep();

        else if (currentStep == 1 && Input.GetKeyDown(KeyCode.Space))
            NextStep();

        else if (currentStep == 2 && Input.GetKeyDown(KeyCode.LeftShift))
            NextStep();

        else if (currentStep == 3 && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
            NextStep();

        else if (currentStep == 4 && Input.GetMouseButtonDown(0))
            NextStep();

        else if (currentStep == 5 && Input.GetKeyDown(KeyCode.R))
            NextStep();

        else if (currentStep == 6 && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)))
            NextStep();

        else if (currentStep == 7 && Input.GetMouseButtonDown(1))
            NextStep();

        else if (currentStep == 8 && Input.GetKeyDown(KeyCode.Q))
            NextStep();

        else if (currentStep == 9 && Input.GetKeyDown(KeyCode.Escape))
            NextStep();

        else if (currentStep == 10 && Input.GetKeyDown(KeyCode.E))
            NextStep();
    }

    void NextStep()
    {
        currentStep++;
        if (currentStep < tutorialMessages.Length)
        {
            tutorialText.text = tutorialMessages[currentStep];
        }
        else
        {
            tutorialText.text = "";
            this.enabled = false; 
        }
    }
}
