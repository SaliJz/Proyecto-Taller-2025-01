using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public GameObject introPanel; 
    public float displayTime = 5f; 
    public string nextSceneName; 

    void Start()
    {
        StartCoroutine(ShowIntro());
    }



    IEnumerator ShowIntro()
    {
        introPanel.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        introPanel.SetActive(false);
        SceneManager.LoadScene(nextSceneName); 
    }
}
