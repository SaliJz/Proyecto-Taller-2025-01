using System.Collections;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    private int currentSceneIndex;
    private bool activated = false;
    TutorialManager manager;

    private void Start()
    {
        manager = TutorialManager.Instance;
       
    }
    IEnumerator DelayStart()
    {
        currentSceneIndex = manager.currentSceneIndex;
        if (currentSceneIndex == 0)
        {
            yield return new WaitForSeconds(2);
            
        }
        else if (currentSceneIndex == 1)
        {
            yield return new WaitForSeconds(0.5f);
        }
        manager.ScenarioActivationCheckerByZones();
        Destroy(gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        currentSceneIndex = manager.currentSceneIndex;
        if (activated) return;
        if (!TutorialManager.Instance.IsTutorialScenePlaying)
        {
            if (other.CompareTag("Player")) 
            {   activated = true;
                if (currentSceneIndex == 0)
                {
                    manager.StartCoroutine(manager.TemporarilyDisablePlayerScripts());
                }     
                StartCoroutine(DelayStart());   
            }
        }     
    }
}