using UnityEngine;

public class ForzarDeltaUno : MonoBehaviour
{
    void Start()
    {
        // Hace que Time.deltaTime valga 1 en el siguiente frame
        Time.timeScale = 1f ;
    }
}
