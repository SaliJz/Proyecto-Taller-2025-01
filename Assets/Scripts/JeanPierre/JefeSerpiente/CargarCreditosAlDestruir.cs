using UnityEngine;
using UnityEngine.SceneManagement;

public class CargarCreditosAlDestruir : MonoBehaviour
{
    // Este método se invoca cuando el GameObject al que está adjunto este componente
    // es destruido (por ejemplo, con Destroy(gameObject)).
    private void OnDestroy()
    {
        // Verificamos que la escena "Creditos" exista en Build Settings
        // y luego la cargamos de inmediato.
        // Nota: Si llamas a Destroy(this.gameObject) desde otro script,
        // tan pronto empiece la destrucción de este objeto se ejecutará OnDestroy.
        SceneManager.LoadScene("Creditos");
    }
}
