using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject loadSceneObject;

    private bool hasTeleported = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTeleported) return;

        if (other.CompareTag(playerTag))
        {
            hasTeleported = true;

            // 1. Desactivar movimiento del jugador
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }

            if (loadSceneObject != null)
            {
                var loader = loadSceneObject.GetComponent<SceneTransition>();
                if (loader != null)
                {
                    loader.LoadSceneWithFade(sceneToLoad);
                    // 3. Desactivar el teletransportador para evitar múltiples activaciones
                    gameObject.SetActive(false);
                    Debug.Log("Teletransportador activado, cargando escena: " + sceneToLoad);
                }
                else
                {
                    Debug.LogError("El objeto 'LoadScene' no tiene el componente SceneLoader.");
                }
            }
            else
            {
                Debug.LogError("No se encontró el objeto 'LoadScene' en la escena.");
            }
        }
    }
}
