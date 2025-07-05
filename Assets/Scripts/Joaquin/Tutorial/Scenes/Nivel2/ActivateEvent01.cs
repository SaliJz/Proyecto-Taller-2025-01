using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateEvent01 : MonoBehaviour
{
    TutorialManager2 tutorialManager;
    float delayStart =8;
    void Start()
    {
        tutorialManager = GetComponent<TutorialManager2>();  
        StartCoroutine(ActivateScene01());
    }

  IEnumerator ActivateScene01()
    {
        tutorialManager.DisablePlayerScriptsForCameraTransition();
        yield return new WaitForSecondsRealtime(delayStart);
        Debug.Log("ddlat");
        tutorialManager.EnablePlayerScriptsAfterCameraTransition();
        tutorialManager.ScenarioActivationCheckByManually();
        yield return new WaitForSecondsRealtime(5);
        tutorialManager.ScenarioActivationCheckByManually();
    }
}
