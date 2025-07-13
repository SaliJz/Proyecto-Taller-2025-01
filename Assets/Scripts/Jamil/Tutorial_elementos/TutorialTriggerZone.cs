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

            if (!TutorialManager0.Instance.IsDialoguePlaying)
            {
                if (other.CompareTag("Player"))
                {
                    if (currentDialogue == 0)
                    {
                        Debug.Log("Activando escenario por zona");
                        activated = true;
                        manager.ScenarioActivationCheckerByZones();
                        manager.StartCoroutine(manager.TemporarilyDisablePlayerScripts(6.5f));
                    }

                    else if (currentDialogue == 2)
                    {
                        Debug.Log("Activando escenario por zona");
                        activated = true;
                        manager.ConfirmAdvance();
                    }

                    else if (currentDialogue == 3)
                    {
                        Debug.Log("Activando escenario por zona");
                        activated = true;
                        manager.ConfirmAdvance();
                    }

                    else if (currentDialogue == 5)
                    {
                        manager.DisablePlayerScriptsForCameraTransition();
                        manager.StartCoroutine(manager.ActivateTransitionBetweenTwoCameras(1, 3, 0, 0));
                        //StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 0));
                        StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 5.22f)); //Activamos la gun
                     
                        activated = true;
                        //Destroy(gameObject);
                    }

                    else if (currentDialogue == 6)
                    {
                        StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 2f));
                        activated = true;
                    }

                    else if (currentDialogue == 7)
                    {
                        StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 2f));
                        activated = true;
                    }

                    else if (currentDialogue == 8)
                    {
                        StartCoroutine(StartDelayConfirmAdvance(manager.ConfirmAdvance, 2f));
                        activated = true;
                    }

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