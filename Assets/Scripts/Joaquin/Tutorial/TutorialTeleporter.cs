using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTeleporter : MonoBehaviour
{
    [SerializeField] private int indexScene = 8;
    private bool activated = false;

    private void OnEnable()
    {
        if (activated) return;
        else
        {
            TutorialManager.Instance.StartScenarioByManual(indexScene);
            Destroy(gameObject);
            activated = true;
        }
    }
}
