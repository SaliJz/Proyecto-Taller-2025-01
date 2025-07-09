using System.Collections;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    private int currentDialogue;
    private bool activated = false;
    TutorialManager0 manager;

    private void Start()
    {
        manager = TutorialManager0.Instance;
        manager.OnPlayerArrivedToCenter += LoadNextDialog;
    }
    void OnDestroy()
    {
        if (manager != null)
            manager.OnPlayerArrivedToCenter -= LoadNextDialog;
    }

    void LoadNextDialog()
    {
        manager.ConfirmAdvance();
    }



//IEnumerator DelayStartScenario()
//{
//    currentDialogueIndex = manager.currentDialogueIndex;
//    if (currentDialogueIndex == 0)
//    {
//        yield return new WaitForSeconds(2);

//    }
//    else if (currentDialogueIndex == 1)
//    {
//        yield return new WaitForSeconds(0.4f);
//        manager.tutorialSceneController.haloMoveController.gameObject.SetActive(false);        
//    }
//    manager.ScenarioActivationCheckerByZones();
//    Destroy(gameObject);
//}
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

                    else if (currentDialogue == 4)
                    {
                        //manager.StartCoroutine(manager.TemporarilyDisablePlayerScripts(20));
                     
                        activated = true;
                    }
                    Destroy(gameObject);
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