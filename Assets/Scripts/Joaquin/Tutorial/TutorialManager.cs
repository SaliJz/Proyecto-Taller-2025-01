using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

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
    [SerializeField] private AudioSource voiceAudioSource;
    //[SerializeField] private float textTypingSpeed = 0.04f;
    [SerializeField] private TutorialSceneController tutorialSceneController;
    [SerializeField] private GameObject FragmentOfTheTutorial;
    [SerializeField] private List<CinemachineVirtualCamera> listVirtualCameras;
    [SerializeField] private List<CinemachineVirtualCamera> listVirtualCamerasCinematic;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private List<MonoBehaviour> playerScriptsToDisable;
    [SerializeField] private GameObject crossHairObject;
    [SerializeField] private Glitch_Cinematic glitchScript;
    private GameObject player;
    public bool IsTutorialScenePlaying => GetTutorialCurrentScene().isActive;
    private int[] killCounters;
    private int[] fragmentCounters;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        killCounters = new int[tutorialScenes.Count];
        fragmentCounters = new int[tutorialScenes.Count];

        tutorialCurrentScene=tutorialScenes[currentSceneIndex];
    }

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

    public void ScenarioActivationCheckerByKills()
    {
        if (GetCurrentSceneActivationType() == ActivationType.ByKills)
        {
            IncreaseKillCounterInCurrentScene(); //Las muertes de enemigo aumentan el contador, para activar escena
            Debug.Log($"Kills para escena {currentSceneIndex}:" +
                $" {killCounters[currentSceneIndex]}/{GetTutorialCurrentScene().tutorialSceneData.necessarykills}");
            if (killCounters[currentSceneIndex] == GetTutorialCurrentScene().tutorialSceneData.necessarykills)
            {
                ActivateCurrentScenario();
                Debug.Log("Conseguiste las kills necesarias");
            }
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
        if(currentSceneIndex == 4) 
        {
            glitchScript.transform.gameObject.SetActive(true);
        }
        ActivateCurrentScenario();
    }

    public void ScenarioActivationCheckByManually()
    {    
        if (GetCurrentSceneActivationType() == ActivationType.ByEventoManual)
        {
            ActivateCurrentScenario();
        }
    }

    public void ScenarioActivationCheckByFragment()
    {    
        if (GetCurrentSceneActivationType() == ActivationType.ByFragments)
        {
            IncreaseFragmentCounterInCurrentScene();
            Debug.Log($"Fragmentos para escena {currentSceneIndex}: " +
                $"{fragmentCounters[currentSceneIndex]}/{GetTutorialCurrentScene().tutorialSceneData.necessaryFragments}");
           
            if (fragmentCounters[currentSceneIndex] == GetTutorialCurrentScene().tutorialSceneData.necessaryFragments)
            {
                ActivateCurrentScenario();
            }
        }
    }

    public void ActivateCurrentScenario()
    {
        //Debug.Log("Escenario activado en índice: " + currentSceneIndex);
        if (GetTutorialCurrentScene().isActive) return; //Aqui, para que la activacion por kills no llame varias veces
        GetTutorialCurrentScene().isActive = true;
        //StopAllCoroutines();
        StartCoroutine(PlayScenarioSequence());

        //if (currentSceneIndex == 2)
        //{
        //    SelectCameraToRender(1);
        //}
    }

    private IEnumerator PlayScenarioSequence()
    {
        var runtimeScene = GetTutorialCurrentScene();
        runtimeScene.onSceneStart?.Invoke();

        DetectCameraSceneTransition();
        foreach (var dialogue in runtimeScene.tutorialSceneData.dialogues)
        {
            if (dialogue.dialogueVoice != null)
                SetTextUI(dialogue.dialogueText);
                voiceAudioSource.PlayOneShot(dialogue.dialogueVoice);
                yield return new WaitForSecondsRealtime(dialogue.dialogueVoice.length);              
        }
        if (currentSceneIndex==4) //la de la cinematica del glitch
        {
            tutorialSceneController.StopGlitchDeathCinematic();
        }

        if(currentSceneIndex==6) //activamos el fragmento
        {
            FragmentOfTheTutorial.SetActive(true);
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
          ScenarioActivationCheckByManually();
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
        dialogueTextUI.text =text;
    }
    void IncreaseCurrentSceneIndex()
    {
        currentSceneIndex+= 1;
    }
    void IncreaseKillCounterInCurrentScene()
    {
        killCounters[currentSceneIndex]+= 1;
    }
    void IncreaseFragmentCounterInCurrentScene()
    {
        fragmentCounters[currentSceneIndex]+= 1;
    }
    private bool HasNextScene()
    {
        return  currentSceneIndex < tutorialScenes.Count;
    }
    public TutorialSceneRuntime GetTutorialCurrentScene()
    {
        return tutorialScenes[currentSceneIndex];
    }
    public ActivationType GetCurrentSceneActivationType()
    {
        return tutorialScenes[currentSceneIndex].tutorialSceneData.activationType;
    }
    private void SelectCameraToRender(int indexCamera)
    {
        ReturnCamerasToDefault();
        listVirtualCameras[indexCamera].Priority = 3;
    }

    private void SelectCameraToRenderCinematic(int indexCamera)
    {
        ReturnCamerasToDefault();
        listVirtualCamerasCinematic[indexCamera].Priority = 4;
    }

    private void ReturnCamerasToDefault()
    {
        foreach (CinemachineVirtualCamera camera in listVirtualCameras)
        {
            if (camera !=listVirtualCameras[2])
            {
                camera.Priority = 1;
            }
        }

        foreach (CinemachineVirtualCamera camera in listVirtualCamerasCinematic)
        {
                camera.Priority = 1;      
        }
    }
    private void DetectCameraSceneTransition()
    {
        switch (currentSceneIndex)
        {      
            case 1: StartCoroutine(ActivateTransitionBetweenCameras()); break;                          
            case 4:
                    cinemachineBrain.m_DefaultBlend= new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
                    glitchScript.StartMovingToB();
                    StartCoroutine(ActivateTransitionBetweenCamerasCinematic()); break;
            case 7: StartCoroutine(ActivateTransitionBetweenCameras()); break;
        }
    }

    public IEnumerator TemporarilyDisablePlayerScripts()
    {
        DisablePlayerScriptsForCameraTransition();
        yield return new WaitForSeconds(5);
        EnablePlayerScriptsAfterCameraTransition();
    }
    IEnumerator ActivateTransitionBetweenCameras()
    {
       DisablePlayerScriptsForCameraTransition();
       SelectCameraToRender(0);
       yield return new WaitForSecondsRealtime(2);
       SelectCameraToRender(1);
       yield return new WaitForSecondsRealtime(2.5f);
       ReturnCamerasToDefault();
       yield return new WaitForSecondsRealtime(2);
       EnablePlayerScriptsAfterCameraTransition();
       tutorialSceneController.EnableGlitchScripts();
    }

    IEnumerator ActivateTransitionBetweenCamerasCinematic()
    {
        DisablePlayerScriptsForCameraTransition();
        SelectCameraToRenderCinematic(0);
        yield return new WaitForSecondsRealtime(2f);
        SelectCameraToRenderCinematic(1);
        yield return new WaitForSecondsRealtime(1.5f);
        SelectCameraToRenderCinematic(2);
        yield return new WaitForSecondsRealtime(2f);
        cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
        ReturnCamerasToDefault();
        EnablePlayerScriptsAfterCameraTransition();
        tutorialSceneController.EnableGlitchScriptsInvulnerables();
    }

    void DisablePlayerScriptsForCameraTransition()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        crossHairObject.SetActive(false);
        foreach(MonoBehaviour script in playerScriptsToDisable)
        {
            script.enabled = false;
        }
    }

    void EnablePlayerScriptsAfterCameraTransition()
    {
        crossHairObject.SetActive(true);
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            script.enabled = true;
        }
    }
}
