using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimacionMuerte : MonoBehaviour
{
    // En el Inspector de Unity podrás asignar el nombre de la escena que quieres cargar
    [SerializeField] private string escenaParaTecla8;
    [SerializeField] private string escenaParaTecla9;
    [SerializeField] private string escenaParaTecla0;

    void Update()
    {
        // Comprobamos si se mantiene presionada la tecla "L"
        bool lPressed = Input.GetKey(KeyCode.L);

        // Si se mantiene "L" y se pulsa la tecla "8"
        if (lPressed && Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (!string.IsNullOrEmpty(escenaParaTecla8))
                SceneManager.LoadScene(escenaParaTecla8);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para L+8.");
        }
        // Si se mantiene "L" y se pulsa la tecla "9"
        else if (lPressed && Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (!string.IsNullOrEmpty(escenaParaTecla9))
                SceneManager.LoadScene(escenaParaTecla9);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para L+9.");
        }
        // Si se mantiene "L" y se pulsa la tecla "0"
        else if (lPressed && Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (!string.IsNullOrEmpty(escenaParaTecla0))
                SceneManager.LoadScene(escenaParaTecla0);
            else
                Debug.LogWarning("No se ha asignado ninguna escena para L+0.");
        }
    }
}
