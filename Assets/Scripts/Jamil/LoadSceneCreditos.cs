using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneCreditos : MonoBehaviour
{
    SceneManager sceneManager;

    public void loadSceneCreditos()
    {
        SceneManager.LoadScene("Creditos");
    }
}
