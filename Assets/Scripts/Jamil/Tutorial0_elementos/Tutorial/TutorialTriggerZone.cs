using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TutorialTriggerZone : MonoBehaviour
{
    private int currentDialogue;
    private bool activated = false;
    TutorialManager0 manager;

    private void Start()
    {
        manager = TutorialManager0.Instance;
    }

    IEnumerator StartDelayConfirmAdvance(Action action, float time)
    {
        Debug.Log("Esperando para confirmar avance...");
        yield return new WaitForSeconds(time);
        Debug.Log("Ejecutando acción...");
        if (manager.currentDialogueIndex == 6)
        {
            TutorialManager0.Instance.spriteJumpToUIs[3].gameObject.SetActive(true);
            manager.ActiveHUD();
            manager.ActiveGun();
            if (manager.wallHologram != null)
            {
                manager.wallHologram.SetActive(true);
                Debug.Log("wallHologram activado");
            }
            else
            {
                Debug.LogWarning("wallHologram NO está asignado en el inspector");
            }
        }
        action?.Invoke();

        
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Entrando a OnTriggerStay");

        if (manager.listDialogueData != null)
        {
            currentDialogue = manager.currentDialogueIndex;
            if (activated)
            {
                Debug.Log("Ya fue activado.");
                return;
            }

            if (!TutorialManager0.Instance.IsDialoguePlaying && TutorialManager0.Instance!=null)
            {
                if (other.CompareTag("Player"))
                {
                    if (currentDialogue == 0) //Bienvenido
                    {
                        activated = true;
                        manager.ScenarioActivationCheckerByZones(); //Activa tras un retraso el dialogo 0
                        manager.StartCoroutine(manager.TemporarilyDisablePlayerScripts(6.5f)); // Tiene un if que al finalizar en indice 0 activa el dialogo 1
                    }

                    //El WASD se detecta en el manager y pasa al dialogo 2, tmb se va el elemento 0
               
                    else if (currentDialogue == 2) //Presiona spacio
                    {
                        activated = true;
                        TutorialManager0.Instance.spriteJumpToUIs[2].gameObject.SetActive(true);
                        TutorialManager0.Instance.spriteJumpToUIs[1].ejecutarAnimacion = true;
                        TutorialManager0.Instance.spriteJumpToUIs[1].GetComponent<SpriteRenderer>().enabled = false;
                        manager.ConfirmAdvance(); //Pasamos al dialogo 3
                    }

                    else if (currentDialogue == 3) //Presion shift
                    {
                        TutorialManager0.Instance.spriteJumpToUIs[2].ejecutarAnimacion = true;
                        TutorialManager0.Instance.spriteJumpToUIs[2].GetComponent<SpriteRenderer>().enabled = false;
                        manager.ConfirmAdvance(); //Pasamos al dialogo 4
                        activated = true;
                    }

                    else if (currentDialogue == 4)// Ve al elevador
                    {
                        manager.ConfirmAdvance(); //Pasamos al dialogo 5
                        activated = true;
                    }
                    //Se queda en dialogo 5  hasta que toque el suelo de arriba 

                    else if (currentDialogue == 5)//Pronto entenderas
                    {
                        manager.DisablePlayerScriptsForCameraTransition();
                        manager.StartCoroutine(manager.ActivateTransitionBetweenTwoCameras(1, 4, 0, 0)); //Ejecutamos cinematica del glitch
                        StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 6.22f)); // Espéramos que termine y activamos la gun
                        activated = true;
                    }
                    // Se queda en dialogo 5 hasta el fade in termine y avanza por este al dialogo 6 (Esta es una de las amaenazas)
                    // Se queda en dialogo 6 hasta que termine la cineamtica del glitch y avanza por el if anterior al dialogo 7 (Dispara al glitch)
                    //Se queda en dialogo 7 esperando a que mate al glitch
                    // Si mata al glitch se inicia la cinematica y avanza al dialogo 8 (acercate a la caja) a la vez
                    //Si toca la caja se activa el dialogo 9 (Bien ahora cambia de arma)
                    //Si cambia de arma se activa el dialogo 10 (Si el enemigo tiene el mismo color que tu arma)
                    //Cuando mate al primer enemigo se activa el dialogo 11 (Cuando mueren dejan fragmentos)
                    //Cuando recoja 7 fragmentos abre la tienda
                  
                }
            }
            else
            {
                Debug.Log("El diálogo ya está activo.");
            }
        }
        else
        {
            Debug.Log("La lista de diálogos es null");
        }
    }
}