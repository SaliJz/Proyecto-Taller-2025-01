//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TutorialFragmentActivator : MonoBehaviour
//{
//    [SerializeField] private float delayBeforeActivation = 0f; // Tiempo de espera antes de poder activarse
//    PickupItem fragmentPickup;

//    private bool isInteractable = false;

//    private void OnEnable()
//    {
//        fragmentPickup = GetComponent<PickupItem>();
//        if (TutorialManager.Instance != null)
//        {
//            StartCoroutine(ActivateFragmentAfterDelay());
//        }
//        else
//        {
//            Debug.LogWarning("TutorialManager.Instance es null en OnEnable de TutorialFragmentActivator.");
//        }
//    }

//    private IEnumerator ActivateFragmentAfterDelay()
//    {    
//        fragmentPickup.enabled = false; // Desactiva el PickupItem para evitar interacciones prematuras

//        yield return new WaitForSeconds(delayBeforeActivation);

//        fragmentPickup.enabled = true; // Reactiva el PickupItem para permitir la recogida

//        // Si necesitas ejecutar algo más tras el delay (como activar efectos visuales), hazlo aquí
//        Debug.Log($"[TutorialFragmentActivator] Fragmento activado tras {delayBeforeActivation} segundos.");
//    }

//    private void OnDestroy()
//    {
//          TutorialManager.Instance.ScenarioActivationCheckByFragment();
//          Debug.Log("Activamos el escenario por fragmentos");
       
//    }
//}
