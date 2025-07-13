using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptRemovalWatcher : MonoBehaviour
{
    [Tooltip("Arrastra aquí el componente (script) que quieres vigilar")]
    public MonoBehaviour targetScript;

    // Para evitar cargas múltiples
    private bool hasLoaded = false;

    void Update()
    {
        // Cuando targetScript se vuelve null (ha sido eliminado), y aún no hemos cargado la escena
        if (!hasLoaded && targetScript == null)
        {
            hasLoaded = true;
            // Asegúrate de que la escena "Creditos" esté en Build Settings
            SceneManager.LoadScene("Creditos");
        }
    }
}
