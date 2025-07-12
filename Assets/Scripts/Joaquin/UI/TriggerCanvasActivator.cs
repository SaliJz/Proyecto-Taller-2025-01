using UnityEngine;

public class TriggerCanvasActivator : MonoBehaviour
{
    [SerializeField] private GameObject canvasAbilitySelector;
    [SerializeField] private GameObject canvasUI;
    [SerializeField] private float gizmosRange;

    private void Start()
    {
        if (canvasAbilitySelector == null)
        {
            Debug.LogError("Canvas Ability Selector no está asignado en TriggerCanvasActivator.");
            return;
        }

        if (canvasUI == null)
        {
            Debug.LogError("Canvas HUD no está asignado en TriggerCanvasActivator.");
            return;
        }

        canvasAbilitySelector.SetActive(false);
        canvasUI.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerCanvasActivator: OnTriggerEnter");
        if (other.GetComponent<PlayerHealth>() != null)
        {
            Debug.Log("Player ha entrado en el trigger");
            ToggleCanvas(canvasUI, false);
            ToggleCanvas(canvasAbilitySelector, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerCanvasActivator: OnTriggerExit");
        if (other.GetComponent<PlayerHealth>() != null)
        {
            Debug.Log("Player ha salido del trigger");
            ToggleCanvas(canvasUI, true);
            ToggleCanvas(canvasAbilitySelector, false);
        }
    }

    private void ToggleCanvas(GameObject canvas, bool state)
    {
        if (canvas != null)
        {
            canvas.SetActive(state);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gizmosRange);
    }
}
