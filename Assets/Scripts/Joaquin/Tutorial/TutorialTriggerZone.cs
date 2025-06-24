using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    public int sceneIndex;
    private bool activated = false;
    TutorialManager manager;

    private void Start()
    {
        manager = TutorialManager.Instance;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!TutorialManager.Instance.IsTutorialScenePlaying)
        {
            if (other.CompareTag("Player")) 
            {
                manager.ScenarioActivationCheckerByZones();
                Destroy(gameObject);
            }
        }
       
    }
}