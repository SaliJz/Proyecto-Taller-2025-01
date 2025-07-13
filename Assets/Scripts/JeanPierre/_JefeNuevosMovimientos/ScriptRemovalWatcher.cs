using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptRemovalOrDisableWatcher : MonoBehaviour
{
    [Tooltip("Arrastra aqu� el componente (script) que quieres vigilar")]
    public MonoBehaviour targetScript;

    // Para evitar cargas m�ltiples
    private bool hasLoaded = false;

    void Update()
    {
        if (hasLoaded)
            return;

        // Si el script ha sido eliminado (null) o est� desactivado (enabled == false)
        bool removedOrDisabled = targetScript == null || !targetScript.enabled;

        if (removedOrDisabled)
        {
            hasLoaded = true;
            // Aseg�rate de que la escena "Creditos" est� a�adida en Build Settings
            SceneManager.LoadScene("Creditos");
        }
    }
}
