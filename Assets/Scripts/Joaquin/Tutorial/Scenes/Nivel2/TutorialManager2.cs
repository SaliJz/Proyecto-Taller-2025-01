using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager2 : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSceneRuntime
    {
        public TutorialSceneData tutorialSceneData;
        public UnityEvent onSceneStart;
        [HideInInspector] public bool isActive = false;
    }

    public List<TutorialSceneRuntime> tutorialScenes;
    public int currentSceneIndex = 0;
    private TutorialSceneRuntime tutorialCurrentScene;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private GameObject crossHairObject;
    [SerializeField] private List<MonoBehaviour> playerScriptsToDisable;
    //[SerializeField] private float textTypingSpeed = 0.04f;
    //[SerializeField] private GameObject FragmentOfTheTutorial;
    private GameObject player;
   

 

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }
    public void ScenarioActivationCheckerByZones()
    {
        if (GetCurrentSceneActivationType() == ActivationType.ByZona) //Solo esto, ya que verificar la colision por trigger se hace en otro script
        {
            ActivateCurrentScenario();
        }
    }

 
    public void ScenarioActivationCheckerByTime()
    {
        if (GetCurrentSceneActivationType() == ActivationType.ForTime)
        {
            float waitTimetoActivateScene = tutorialCurrentScene.tutorialSceneData.necessaryTime;
            StartCoroutine(WaitToActivateScenario(waitTimetoActivateScene));
        }
    }

    private IEnumerator WaitToActivateScenario(float time)
    {
        yield return new WaitForSeconds(time);
        ActivateCurrentScenario();
    }

    public void ScenarioActivationCheckByManually()
    {
        if (GetCurrentSceneActivationType() == ActivationType.ByEventoManual)
        {
            ActivateCurrentScenario();
        }
    }

    public void ActivateCurrentScenario()
    {
        //Debug.Log("Escenario activado en índice: " + currentSceneIndex);
        if (GetTutorialCurrentScene().isActive) return; //Aqui, para que la activacion por kills no llame varias veces
        GetTutorialCurrentScene().isActive = true;
        //StopAllCoroutines();
        StartCoroutine(PlayScenarioSequence());
    }

    private IEnumerator PlayScenarioSequence()
    {
        var runtimeScene = GetTutorialCurrentScene();
        runtimeScene.onSceneStart?.Invoke();
       
        foreach (var dialogue in runtimeScene.tutorialSceneData.dialogues)
        {
            SetTextUI(dialogue.dialogueText);
           
                yield return new WaitForSecondsRealtime(5);
                SetTextUI("");

        }

        HandleScenarioCompletion();
    }

    private void HandleScenarioCompletion()
    {
        GetTutorialCurrentScene().isActive = false;
        IncreaseCurrentSceneIndex();

        if (HasNextScene()) //Verificamos que la escena exista
        {
            tutorialCurrentScene = GetTutorialCurrentScene(); //Actualizamos la escena con la escena del indice actual

            //Estos metodos se activan automaticamente al comprobar el indice actual, el cual fue incrementado previamente en 1
            ScenarioActivationCheckerByTime();
            //ScenarioActivationCheckByManually();
        }
    }
    //private IEnumerator AnimateTextTyping(string text)
    //{
    //    Debug.Log("TEXTO: " + text);
    //    dialogueTextUI.text = "";

    //    foreach (char character in text)
    //    {
    //        dialogueTextUI.text += character;
    //        yield return new WaitForSecondsRealtime(textTypingSpeed);
    //    }
    //}
    private void SetTextUI(string text)
    {
        dialogueTextUI.text = text;
    }
    void IncreaseCurrentSceneIndex()
    {
        currentSceneIndex += 1;
    }
   
    private bool HasNextScene()
    {
        return currentSceneIndex < tutorialScenes.Count;
    }
    public TutorialSceneRuntime GetTutorialCurrentScene()
    {
        return tutorialScenes[currentSceneIndex];
    }
    public ActivationType GetCurrentSceneActivationType()
    {
        return tutorialScenes[currentSceneIndex].tutorialSceneData.activationType;
    }
    public void DisablePlayerScriptsForCameraTransition()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No se encontró un GameObject con la etiqueta 'Player'.");
                return;
            }
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        crossHairObject.SetActive(false);

        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }
    }

    public void EnablePlayerScriptsAfterCameraTransition()
    {
        crossHairObject.SetActive(true);
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            script.enabled = true;
        }
    }
}
