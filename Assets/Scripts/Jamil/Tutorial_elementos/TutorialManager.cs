//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using Cinemachine;
//public class TutorialManager : MonoBehaviour
//{
//    public static TutorialManager Instance { get; private set; }

//    [System.Serializable]
//    public class TutorialSceneRuntime
//    {
//        public TutorialSceneData tutorialSceneData;
//        public UnityEvent onSceneStart;
//        [HideInInspector] public bool isActive = false;
//    }

//    public List<TutorialSceneRuntime> tutorialScenes;
//    public int currentDialogue = 0;
//    private TutorialSceneRuntime tutorialCurrentScene; 
//    [SerializeField] private TextMeshProUGUI dialogueTextUI;
//    [SerializeField] private AudioSource voiceAudioSource;
//    //[SerializeField] private float textTypingSpeed = 0.04f;
//    [SerializeField] public TutorialSceneController tutorialSceneController;
//    //[SerializeField] private GameObject FragmentOfTheTutorial;
//    [SerializeField] private List<CinemachineVirtualCamera> listVirtualCameras;
//    [SerializeField] private List<CinemachineVirtualCamera> listVirtualCamerasCinematic;
//    [SerializeField] private CinemachineBrain cinemachineBrain;
//    [SerializeField] private List<MonoBehaviour> playerScriptsToDisable;
//    [SerializeField] private GameObject crossHairObject;
//    [SerializeField] private Glitch_Cinematic glitchScript;
//    private GameObject player;
//    public bool IsTutorialScenePlaying => GetCurrentDialogueData().isActive;
//    private int[] killCounters;
//    private int[] fragmentCounters;

//    private void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);

//        killCounters = new int[tutorialScenes.Count];
//        fragmentCounters = new int[tutorialScenes.Count];

//        tutorialCurrentScene=tutorialScenes[currentDialogue];
//    }

//    private void Start()
//    {
//        player = GameObject.FindWithTag("Player");
//        if (cinemachineBrain == null) return;
//        cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
//    }
//    public void ScenarioActivationCheckerByZones()
//    {
//        if (GetCurrentSceneActivationType() == ActivationType.ByZona) //Solo esto, ya que verificar la colision por trigger se hace en otro script
//        {
//            ActivateCurrentScenario();
//        }
//    }

//    public void ScenarioActivationCheckerByKills()
//    {
//        if (GetCurrentSceneActivationType() == ActivationType.ByKills)
//        {
//            IncreaseKillCounterInCurrentScene(); //Las muertes de enemigo aumentan el contador, para activar escena
//            Debug.Log($"Kills para escena {currentDialogue}:" +
//                $" {killCounters[currentDialogue]}/{GetCurrentDialogueData().tutorialSceneData.necessarykills}");
//            if (killCounters[currentDialogue] == GetCurrentDialogueData().tutorialSceneData.necessarykills)
//            {
//                ActivateCurrentScenario();
//                Debug.Log("Conseguiste las kills necesarias");
//            }
//        }
//    }

//    public void ScenarioActivationCheckerByTime()
//    {
//        if (GetCurrentSceneActivationType() == ActivationType.ForTime)
//        {
//            float waitTimetoActivateScene = tutorialCurrentScene.tutorialSceneData.necessaryTime;
//            StartCoroutine(WaitToActivateScenario(waitTimetoActivateScene));
//        }
//    }

//    private IEnumerator WaitToActivateScenario(float time)
//    {
//        yield return new WaitForSeconds(time);   
//        if(currentDialogue == 4) 
//        {
//            glitchScript.transform.gameObject.SetActive(true);
//        }
//        ActivateCurrentScenario();
//    }

//    public void ScenarioActivationCheckByManually()
//    {    
//        if (GetCurrentSceneActivationType() == ActivationType.ByEventoManual)
//        {
//            ActivateCurrentScenario();
//        }
//    }

//    public void ScenarioActivationCheckByFragment()
//    {    
//        if (GetCurrentSceneActivationType() == ActivationType.ByFragments)
//        {
//            IncreaseFragmentCounterInCurrentScene();
//            Debug.Log($"Fragmentos para escena {currentDialogue}: " +
//                $"{fragmentCounters[currentDialogue]}/{GetCurrentDialogueData().tutorialSceneData.necessaryFragments}");

//            if (fragmentCounters[currentDialogue] == GetCurrentDialogueData().tutorialSceneData.necessaryFragments)
//            {
//                ActivateCurrentScenario();
//            }
//        }
//    }

//    public void ActivateCurrentScenario()
//    {
//        //Debug.Log("Escenario activado en índice: " + currentDialogueIndex);
//        if (GetCurrentDialogueData().isActive) return; //Aqui, para que la activacion por kills no llame varias veces
//        GetCurrentDialogueData().isActive = true;
//        //StopAllCoroutines();
//        StartCoroutine(PlayScenarioSequence());
//    }

//    private IEnumerator PlayScenarioSequence()
//    {
//        var runtimeScene = GetCurrentDialogueData();
//        runtimeScene.onSceneStart?.Invoke();

//        DetectCameraSceneTransition();
//        if (currentDialogue == 3) //activamos el fragmento
//        {
//            tutorialSceneController.orbitingCircleSpawner.enabled = true;
//        }
//        foreach (var dialogue in runtimeScene.tutorialSceneData.dialogues)
//        {
//            if (dialogue.dialogueText != null)  
//            {
//               SetTextUI(dialogue.dialogueText);
//            }

//            if (dialogue.dialogueVoice != null)
//            {
//                voiceAudioSource.PlayOneShot(dialogue.dialogueVoice);
//                yield return new WaitForSecondsRealtime(dialogue.dialogueVoice.length);
//            }


//        }
//        if (currentDialogue == 3) 
//        {
//            tutorialSceneController.orbitingCircleSpawner.activateScripts=true;
//            tutorialSceneController.orbitingCircleSpawner.prefab.GetComponent<SphereCollider>().enabled = true;

//        }
//        if (currentDialogue == 0)
//        {
//            if (tutorialSceneController == null)
//            {
//                Debug.LogError("tutorialSceneController no está asignado.");
//                //yield break; // Detiene la corrutina si falta el controlador
//            }

//            else
//            {
//                StartCoroutine(ActivateTransitionBetweenTwoCameras(1, 3f, 1, 0));
//                tutorialSceneController.StartHaloCorutine();
//            }


//        }
//        if (currentDialogue == 1)
//        {
//            Debug.Log("ActivandoGlitchNormal");
//            tutorialSceneController.EnableGlitchScripts();
//        }

//        if (currentDialogue == 4)
//        {
//            tutorialSceneController.EnableGlitchScriptsInvulnerables();
//        }

//        HandleScenarioCompletion();
//    }

//    private void HandleScenarioCompletion()
//    {
//        GetCurrentDialogueData().isActive = false;
//        IncreaseDialogueIndex(); 

//        if (HasNextScene()) //Verificamos que la escena exista
//        {
//            if (currentDialogue < tutorialScenes.Count)
//            {
//                tutorialCurrentScene = GetCurrentDialogueData(); //Actualizamos la escena con la escena del indice actual

//                //Estos metodos se activan automaticamente al comprobar el indice actual, el cual fue incrementado previamente en 1
//                ScenarioActivationCheckerByTime();
//                ScenarioActivationCheckByManually();
//            }

//        }
//    }
//    //private IEnumerator AnimateTextTyping(string text)
//    //{
//    //    Debug.Log("TEXTO: " + text);
//    //    dialogueTextUI.text = "";

//    //    foreach (char character in text)
//    //    {
//    //        dialogueTextUI.text += character;
//    //        yield return new WaitForSecondsRealtime(textTypingSpeed);
//    //    }
//    //}
//    private void SetTextUI(string text)
//    {
//        dialogueTextUI.text =text;
//    }
//    void IncreaseDialogueIndex()
//    {
//        currentDialogue+= 1;
//    }
//    void IncreaseKillCounterInCurrentScene()
//    {
//        killCounters[currentDialogue]+= 1;
//    }
//    void IncreaseFragmentCounterInCurrentScene()
//    {
//        fragmentCounters[currentDialogue]+= 1;
//    }
//    private bool HasNextScene()
//    {
//        return  currentDialogue < tutorialScenes.Count;
//    }
//    public TutorialSceneRuntime GetCurrentDialogueData()
//    {
//        return tutorialScenes[currentDialogue];
//    }
//    public ActivationType GetCurrentSceneActivationType()
//    {      
//       return tutorialScenes[currentDialogue].tutorialSceneData.activationType;

//    }

//    private void SelectCameraToRender(int indexCamera)
//    {
//        ReturnCamerasToDefault();
//        listVirtualCameras[indexCamera].Priority = 3;
//    }

//    private void SelectCameraToRenderCinematic(int indexCamera)
//    {
//        ReturnCamerasToDefault();
//        listVirtualCamerasCinematic[indexCamera].Priority = 4;
//    }

//    private void ReturnCamerasToDefault()
//    {
//        foreach (CinemachineVirtualCamera camera in listVirtualCameras)
//        {
//            if (camera !=listVirtualCameras[0])
//            {
//                camera.Priority = 1;
//            }
//        }

//        foreach (CinemachineVirtualCamera camera in listVirtualCamerasCinematic)
//        {
//                camera.Priority = 1;      
//        }
//    }
//    private void DetectCameraSceneTransition()
//    {
//        switch (currentDialogue)
//        {
//            case 1: StartCoroutine(ActivateTransitionBetweenTwoCameras(2,2,3,2.5f)); break;
//            //case 3: StartCoroutine(ActivateTransitionBetweenTwoCameras(1, 5f, 1, 0)); break;
//            //case 4:
//            //        cinemachineBrain.m_DefaultBlend= new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
//            //        glitchScript.StartMovingToB();
//            //        StartCoroutine(ActivateTransitionBetweenCamerasCinematic()); break;
//            case 4: StartCoroutine(ActivateTransitionBetweenTwoCameras(2,2,3,2.5f)); break;
//        }
//    }

//    public IEnumerator TemporarilyDisablePlayerScripts()
//    {
//        DisablePlayerScriptsForCameraTransition();
//        yield return new WaitForSeconds(6.5f);
//        EnablePlayerScriptsAfterCameraTransition();
//        if (currentDialogue == 0)
//        {
//            yield return new WaitForSeconds(0.4f);
//            tutorialSceneController.DisableTeleport();
//        }
//        EnablePlayerScriptsAfterCameraTransition();
//    }
//    IEnumerator ActivateTransitionBetweenTwoCameras(int camera1,float time1, int camera2, float time2)
//    {
//        if (cinemachineBrain != null)
//        {
//            cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
//        }
//        DisablePlayerScriptsForCameraTransition();
//       tutorialSceneController.ActivateFadeOutOnCameraSwitch();
//        if(currentDialogue == 0)
//        {
//            tutorialSceneController.rotateRig.isActive = true;
//        }  
//       yield return new WaitUntil(() => tutorialSceneController.fadeTransition.isFadeIn == false);
//       SelectCameraToRender(camera1);
//       yield return new WaitForSecondsRealtime(time1);
//       SelectCameraToRender(camera2);
//       yield return new WaitForSecondsRealtime(time2);
//       tutorialSceneController.rotateRig.isActive = false;
//       cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseOut, 2f);
//       ReturnCamerasToDefault();
//       yield return new WaitForSecondsRealtime(2);
//       EnablePlayerScriptsAfterCameraTransition();
//    }
//    void DisablePlayerScriptsForCameraTransition()
//    {
//        Rigidbody rb = player.GetComponent<Rigidbody>();
//        rb.velocity = Vector3.zero;
//        crossHairObject.SetActive(false);
//        foreach(MonoBehaviour script in playerScriptsToDisable)
//        {
//            script.enabled = false;
//        }
//    }

//    void EnablePlayerScriptsAfterCameraTransition()
//    {
//        crossHairObject.SetActive(true);
//        foreach (MonoBehaviour script in playerScriptsToDisable)
//        {
//            script.enabled = true;
//        }
//    }
//}
