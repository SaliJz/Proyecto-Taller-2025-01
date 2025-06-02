using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // En el Inspector de Unity podrás asignar el nombre de la escena que quieres cargar
    [SerializeField] private string escenaParaTecla8;
    [SerializeField] private string escenaParaTecla9;
    [SerializeField] private string escenaParaTecla0;

    void Update()
    {
        // Si se pulsa la tecla "8"
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            // Carga la escena cuyo nombre está en 'escenaParaTecla8'
            if (!string.IsNullOrEmpty(escenaParaTecla8))
                SceneManager.LoadScene(escenaParaTecla8);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para la tecla 8.");
        }
        // Si se pulsa la tecla "9"
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            // Carga la escena cuyo nombre está en 'escenaParaTecla9'
            if (!string.IsNullOrEmpty(escenaParaTecla9))
                SceneManager.LoadScene(escenaParaTecla9);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para la tecla 9.");
        }
        // Si se pulsa la tecla "0"
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            // Carga la escena cuyo nombre está en 'escenaParaTecla0'
            if (!string.IsNullOrEmpty(escenaParaTecla0))
                SceneManager.LoadScene(escenaParaTecla0);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para la tecla 0.");
        }
    }
}
