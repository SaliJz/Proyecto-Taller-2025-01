using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFragments : MonoBehaviour
{
    [SerializeField] private int indexScene = 7;
    [SerializeField] private float activationDelay = 5f; // Tiempo antes de poder activarse

    private bool isActive = false;

    private void OnEnable()
    {
        if (TutorialManager.Instance != null)
        {
            StartCoroutine(DelayedActivation());
        }
        else
        {
            Debug.LogWarning("TutorialManager.Instance es null en OnEnable de TutorialFragments.");
        }
    }

    private IEnumerator DelayedActivation()
    {
        PickupItem pickupItem = GetComponent<PickupItem>();

        isActive = false;
        pickupItem.enabled = false; // Desactiva el PickupItem para evitar interacciones prematuras

        yield return new WaitForSeconds(activationDelay);

        TutorialManager.Instance.StartScenarioByManual(6);

        isActive = true;
        pickupItem.enabled = true; // Reactiva el PickupItem para permitir la recogida

        // Si necesitas ejecutar algo más tras el delay (como activar efectos visuales), hazlo aquí
        Debug.Log($"[TutorialFragments] Fragmento activado tras {activationDelay} segundos.");
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null && isActive)
        {
            Debug.Log($"[TutorialFragments] Fragmento destruido, reportando a escena {indexScene}");
            TutorialManager.Instance.StartScenarioByFragments(indexScene);
        }
    }
}
