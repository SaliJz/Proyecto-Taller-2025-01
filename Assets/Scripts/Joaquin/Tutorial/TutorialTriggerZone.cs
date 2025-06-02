using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    public int sceneIndex;
    private bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.StartScenarioByZone(sceneIndex);
            activated = true;
        }
    }
}