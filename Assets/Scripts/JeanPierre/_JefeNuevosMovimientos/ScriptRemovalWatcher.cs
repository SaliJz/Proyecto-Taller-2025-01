using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptRemovalOrDisableWatcher : MonoBehaviour
{
    [Tooltip("Arrastra aquí el componente (script) que quieres vigilar")]
    public MonoBehaviour targetScript;

    // Para evitar cargas múltiples
    private bool hasLoaded = false;

    void Update()
    {
        if (hasLoaded)
            return;

        // Si el script ha sido eliminado (null) o está desactivado (enabled == false)
        bool removedOrDisabled = targetScript == null || !targetScript.enabled;

        if (removedOrDisabled)
        {
            hasLoaded = true;
            // Asegúrate de que la escena "Creditos" esté añadida en Build Settings
            SceneManager.LoadScene("Creditos");
        }
    }
}
