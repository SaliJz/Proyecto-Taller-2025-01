using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    public int sceneIndex;
    private bool active = false;

    void OnTriggerEnter(Collider other)
    {
        if (active) return;

        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.StartScenarioByZone(sceneIndex);
            active = true;
        }
    }
}
