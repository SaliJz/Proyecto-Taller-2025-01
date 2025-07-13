using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptRemovalWatcher : MonoBehaviour
{
    [Tooltip("Arrastra aqu� el componente (script) que quieres vigilar")]
    public MonoBehaviour targetScript;

    // Para evitar cargas m�ltiples
    private bool hasLoaded = false;

    void Update()
    {
        // Cuando targetScript se vuelve null (ha sido eliminado), y a�n no hemos cargado la escena
        if (!hasLoaded && targetScript == null)
        {
            hasLoaded = true;
            // Aseg�rate de que la escena "Creditos" est� en Build Settings
            SceneManager.LoadScene("Creditos");
        }
    }
}
