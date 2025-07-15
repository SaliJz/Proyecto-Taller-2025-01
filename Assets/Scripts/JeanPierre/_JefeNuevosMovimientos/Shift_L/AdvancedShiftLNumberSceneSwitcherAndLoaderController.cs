using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// AdvancedShiftLNumberSceneSwitcherAndLoaderController
/// ----------------------------------------------------
/// Este componente permite cambiar de escena manteniendo pulsadas las teclas Shift + L,
/// y luego presionando un número entre 0 y 5. Además registra el historial de cambios
/// de escena en consola para facilitar la depuración.
/// </summary>
public class AdvancedShiftLNumberSceneSwitcherAndLoaderController : MonoBehaviour
{
    // Constantes con los nombres de las escenas configuradas en Build Settings
    private const string ESCENA_MENU_PRINCIPAL = "MenuPrincipal";
    private const string ESCENA_NIVEL_1 = "Nivel1";
    private const string ESCENA_NIVEL_2 = "Nivel2";
    private const string ESCENA_NIVEL_3 = "Nivel3";
    private const string ESCENA_NIVEL_4 = "Nivel4";
    private const string ESCENA_JEANPIERRE_JEFE = "JeanPierre_Jefe";

    // Teclas modificadoras requeridas
    private readonly KeyCode[] SHIFT_KEYS = { KeyCode.LeftShift, KeyCode.RightShift };
    private const KeyCode TECLA_L = KeyCode.L;

    void Update()
    {
        if (IsShiftHeld() && Input.GetKey(TECLA_L))
        {
            DetectAndLoadSceneByNumber();
        }
    }

    /// <summary>
    /// Comprueba si alguna de las teclas Shift está siendo presionada.
    /// </summary>
    /// <returns>True si Shift izquierdo o derecho está activo, false en caso contrario.</returns>
    private bool IsShiftHeld()
    {
        foreach (var shiftKey in SHIFT_KEYS)
        {
            if (Input.GetKey(shiftKey))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Detecta la pulsación de una tecla numérica (Alpha0–Alpha5) y carga la escena correspondiente.
    /// </summary>
    private void DetectAndLoadSceneByNumber()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) LoadScene(ESCENA_MENU_PRINCIPAL);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) LoadScene(ESCENA_NIVEL_1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) LoadScene(ESCENA_NIVEL_2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) LoadScene(ESCENA_NIVEL_3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) LoadScene(ESCENA_NIVEL_4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) LoadScene(ESCENA_JEANPIERRE_JEFE);
    }

    /// <summary>
    /// Carga la escena cuyo nombre se proporciona y registra el cambio en la consola.
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar</param>
    private void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneSwitcher] Cargando escena: {sceneName} (teclas Shift + L + número) — BuildIndex: {GetBuildIndex(sceneName)}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Obtiene el índice en Build Settings de una escena por su nombre.
    /// </summary>
    /// <param name="sceneName">Nombre de la escena</param>
    /// <returns>Índice de la escena en Build Settings o -1 si no está presente</returns>
    private int GetBuildIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return i;
        }
        return -1;
    }
}
