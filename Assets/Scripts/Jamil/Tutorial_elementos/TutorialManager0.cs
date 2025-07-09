using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager0 : MonoBehaviour
{
    public static TutorialManager0 Instance { get; private set; }
    [System.Serializable]
    public class TutorialSceneRuntime
    {
        public TutorialSceneData tutorialSceneData;
        public UnityEvent onSceneStart;
    }
    [SerializeField] private TutorialSceneRuntime tutorialSceneRuntime;
    public int currentDialogueIndex = 0;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] public List<DialogueData> listDialogueData = new List<DialogueData>();

    //[SerializeField] public TutorialSceneController tutorialSceneController;
    //[SerializeField] private List<CinemachineVirtualCamera> listVirtualCameras;
    //[SerializeField] private List<CinemachineVirtualCamera> listVirtualCamerasCinematic;
    //[SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private List<MonoBehaviour> playerScriptsToDisable;
    [SerializeField] private GameObject crossHairObject;
    private GameObject player;
    public bool hasConfirmedDialogueAdvance = false;
    private bool isDetectingSpace = false;
    private bool isDetectingWASD = false;
    public event Action onConfirmAdvance;
    public Action OnPlayerArrivedToCenter;
    private bool isPlayerDisabled;
    
    public bool IsDialoguePlaying => GetCurrentDialogueData().isActive;
    private int[] killCounters;
    private int[] fragmentCounters;

    [SerializeField] public Vector3 targetPosition1;
    [SerializeField] public  Vector3 targetPosition2;
    [SerializeField] private float arriveSpeed;



    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Desactivar scripts desde el comienzo
        foreach (var script in playerScriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        listDialogueData = tutorialSceneRuntime.tutorialSceneData.dialogues;
        foreach (var dialogue in listDialogueData)
        {
            dialogue.isActive = false;
        }
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {

        if (currentDialogueIndex == 1)
        {
            IsWASDPressed();
        }

    }
    public void ScenarioActivationCheckerByZones()
    {
        if (GetCurrentDialogueData().activationType == ActivationType.ByZona) //Solo esto, ya que verificar la colision por trigger se hace en otro script
        {
            if (currentDialogueIndex == 0)
            {
                StartCoroutine(DelayStartScenario(2));
            }
            else
            {
                StartCoroutine(DelayStartScenario(0));
            }

        }
    }
    public void ConfirmAdvance()
    {
        onConfirmAdvance?.Invoke();
    }
    //public void ScenarioActivationCheckerByKills()
    //{
    //    if (GetCurrentSceneActivationType() == ActivationType.ByKills)
    //    {
    //        IncreaseKillCounterInCurrentScene(); //Las muertes de enemigo aumentan el contador, para activar escena
    //        Debug.Log($"Kills para escena {currentDialogueIndex}:" +
    //            $" {killCounters[currentDialogueIndex]}/{GetCurrentDialogueData().tutorialSceneData.necessarykills}");
    //        if (killCounters[currentDialogueIndex] == GetCurrentDialogueData().tutorialSceneData.necessarykills)
    //        {
    //            ActivateCurrentScenario();
    //            Debug.Log("Conseguiste las kills necesarias");
    //        }
    //    }
    //}

    //public void ScenarioActivationCheckerByTime()
    //{
    //    if (GetCurrentSceneActivationType() == ActivationType.ForTime)
    //    {
    //        float waitTimetoActivateScene = tutorialCurrentScene.tutorialSceneData.necessaryTime;
    //        StartCoroutine(WaitToActivateScenario(waitTimetoActivateScene));
    //    }
    //}

    //private IEnumerator WaitToActivateScenario(float time)
    //{
    //    yield return new WaitForSeconds(time);
    //    if (currentDialogueIndex == 4)
    //    {
    //        glitchScript.transform.gameObject.SetActive(true);
    //    }
    //    ActivateCurrentScenario();
    //}

    //public void ScenarioActivationCheckByManually()
    //{
    //    if (GetCurrentSceneActivationType() == ActivationType.ByEventoManual)
    //    {
    //        ActivateCurrentScenario();
    //    }
    //}

    //public void ScenarioActivationCheckByFragment()
    //{
    //    if (GetCurrentSceneActivationType() == ActivationType.ByFragments)
    //    {
    //        IncreaseFragmentCounterInCurrentScene();
    //        Debug.Log($"Fragmentos para escena {currentDialogueIndex}: " +
    //            $"{fragmentCounters[currentDialogueIndex]}/{GetCurrentDialogueData().tutorialSceneData.necessaryFragments}");

    //        if (fragmentCounters[currentDialogueIndex] == GetCurrentDialogueData().tutorialSceneData.necessaryFragments)
    //        {
    //            ActivateCurrentScenario();
    //        }
    //    }
    //}

    public void ActivateCurrentScenario()
    {
        //Debug.Log("Escenario activado en índice: " + currentDialogueIndex);
        if (GetCurrentDialogueData().isActive) return; //Aqui, para que la activacion por kills no llame varias veces
        GetCurrentDialogueData().isActive = true;
        //StopAllCoroutines();

        StartCoroutine(PlayScenarioSequence());
    }

    private IEnumerator PlayScenarioSequence()
    {

        tutorialSceneRuntime.onSceneStart?.Invoke();
        //DetectCameraSceneTransition();

        foreach (var dialogue in tutorialSceneRuntime.tutorialSceneData.dialogues)
        {
            hasConfirmedDialogueAdvance = false;

            if (dialogue.dialogueText != null)
            {
                SetTextUI(dialogue.dialogueText);

            }

            if (dialogue.dialogueVoice != null)
            {
                //voiceAudioSource.PlayOneShot(dialogue.dialogueVoice);
                yield return new WaitForSecondsRealtime(dialogue.dialogueVoice.length);
            }
            yield return StartCoroutine(WaitForConfirmation());

            GetCurrentDialogueData().isActive = false;
            IncreaseDialogueIndex();

        }

        HandleScenarioCompletion();
    }

    private IEnumerator WaitForConfirmation()
    {
        hasConfirmedDialogueAdvance = false;
        onConfirmAdvance += Confirmed;
     //La condicion de salto se controla en su propio script
        if (currentDialogueIndex == 3)
        {
            EnabledPlayerDash();
        }

        else if (currentDialogueIndex == 5)
        {
            yield return StartCoroutine(MovePlayerToPlatformCenter(targetPosition1));
        }
        yield return new WaitUntil(() => hasConfirmedDialogueAdvance);
        onConfirmAdvance -= Confirmed;
    }

    private void Confirmed()
    {
        hasConfirmedDialogueAdvance = true;
    }
    private void HandleScenarioCompletion()
    {
        GetCurrentDialogueData().isActive = false;


        if (HasNextDialogue()) //Verificamos que la escena exista
        {
            //if (currentDialogueIndex < tutorialScenes.Count)
            //{
            //    tutorialCurrentScene = GetCurrentDialogueData(); //Actualizamos la escena con la escena del indice actual

            //    //Estos metodos se activan automaticamente al comprobar el indice actual, el cual fue incrementado previamente en 1
            //    ScenarioActivationCheckerByTime();
            //    ScenarioActivationCheckByManually();
            //}

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
    void IncreaseDialogueIndex()
    {
        currentDialogueIndex += 1;
    }
    //void IncreaseKillCounterInCurrentScene()
    //{
    //    killCounters[currentDialogueIndex] += 1;
    //}
    //void IncreaseFragmentCounterInCurrentScene()
    //{
    //    fragmentCounters[currentDialogueIndex] += 1;
    //}
    private bool HasNextDialogue()
    {
        return currentDialogueIndex < listDialogueData.Count;
    }
    public DialogueData GetCurrentDialogueData()
    {
        return listDialogueData[currentDialogueIndex];
    }
    //public ActivationType GetCurrentSceneActivationType()
    //{
    //    return listDialogueData[currentDialogueIndex].tutorialSceneData.activationType;
    //}

    //private void SelectCameraToRender(int indexCamera)
    //{
    //    ReturnCamerasToDefault();
    //    //listVirtualCameras[indexCamera].Priority = 3;
    //}

    //private void SelectCameraToRenderCinematic(int indexCamera)
    //{
    //    ReturnCamerasToDefault();
    //    //listVirtualCamerasCinematic[indexCamera].Priority = 4;
    //}

    //private void ReturnCamerasToDefault()
    //{
    //    foreach (CinemachineVirtualCamera camera in listVirtualCameras)
    //    {
    //        if (camera != listVirtualCameras[0])
    //        {
    //            camera.Priority = 1;
    //        }
    //    }

    //    foreach (CinemachineVirtualCamera camera in listVirtualCamerasCinematic)
    //    {
    //        camera.Priority = 1;
    //    }
    //}
    //private void DetectCameraSceneTransition()
    //{
    //    switch (currentDialogueIndex)
    //    {
    //        case 1: StartCoroutine(ActivateTransitionBetweenTwoCameras(2, 2, 3, 2.5f)); break;
    //        //case 3: StartCoroutine(ActivateTransitionBetweenTwoCameras(1, 5f, 1, 0)); break;
    //        //case 4:
    //        //        cinemachineBrain.m_DefaultBlend= new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
    //        //        glitchScript.StartMovingToB();
    //        //        StartCoroutine(ActivateTransitionBetweenCamerasCinematic()); break;
    //        case 4: StartCoroutine(ActivateTransitionBetweenTwoCameras(2, 2, 3, 2.5f)); break;
    //    }
    //}

    private IEnumerator DelayStartScenario(float time)
    {
        yield return new WaitForSeconds(time);
        ActivateCurrentScenario();
    }
    public IEnumerator TemporarilyDisablePlayerScripts(float time)
    {
        Debug.Log("desactivando");
        isPlayerDisabled = true;
        DisablePlayerScriptsForCameraTransition();
        yield return new WaitForSeconds(time);
        EnablePlayerScriptsAfterCameraTransition();
        isPlayerDisabled = false;

        if (listDialogueData[0].isActive)
        {
            onConfirmAdvance?.Invoke();
        }
    }

    //void IsSpacePressed()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        onConfirmAdvance?.Invoke();
    //    }
    //}

    private void IsWASDPressed()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
       || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            onConfirmAdvance?.Invoke();
        }
    }
    //void IsShiftPressed()
    //{
    //    if (Input.GetKeyDown(KeyCode.LeftShift))
    //    {
    //        onConfirmAdvance?.Invoke();
    //    }
    //}
    //IEnumerator ActivateTransitionBetweenTwoCameras(int camera1, float time1, int camera2, float time2)
    //{
    //    if (cinemachineBrain != null)
    //    {
    //        cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
    //    }
    //    DisablePlayerScriptsForCameraTransition();
    //    tutorialSceneController.ActivateFadeOutOnCameraSwitch();
    //    if (currentDialogueIndex == 0)
    //    {
    //        tutorialSceneController.rotateRig.isActive = true;
    //    }
    //    yield return new WaitUntil(() => tutorialSceneController.fadeTransition.isFadeIn == false);
    //    SelectCameraToRender(camera1);
    //    yield return new WaitForSecondsRealtime(time1);
    //    SelectCameraToRender(camera2);
    //    yield return new WaitForSecondsRealtime(time2);
    //    tutorialSceneController.rotateRig.isActive = false;
    //    cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseOut, 2f);
    //    ReturnCamerasToDefault();
    //    yield return new WaitForSecondsRealtime(2);
    //    EnablePlayerScriptsAfterCameraTransition();
    //}
    void DisablePlayerScriptsForCameraTransition()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        crossHairObject.SetActive(false);
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (currentDialogueIndex == 4 && script is PlayerCamera)
            {
                continue;
            }

            script.enabled = false;
        }
    }

    void EnablePlayerScriptsAfterCameraTransition()
    {
        crossHairObject.SetActive(true);
        foreach (MonoBehaviour script in playerScriptsToDisable)
        { 
            if (currentDialogueIndex<3 && !(script is PlayerDash))
            {
                script.enabled = true;
            }
            
            else if(currentDialogueIndex>=3)
            {
                script.enabled = true;
            }
        }
    }

    void EnabledPlayerDash()
    {
        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (script is PlayerDash)
            {
                script.enabled = true;
            }
        }
    }

    public IEnumerator MovePlayerToPlatformCenter(Vector3 target)
    {
        Vector3 targetXZ = new Vector3(target.x, player.transform.position.y, target.z);

        while (Vector3.Distance(player.transform.position, targetXZ) > 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetXZ, arriveSpeed * Time.deltaTime);
            yield return null;
        }

        // Notifica que el jugador llegó
        OnPlayerArrivedToCenter?.Invoke();
    }
}
