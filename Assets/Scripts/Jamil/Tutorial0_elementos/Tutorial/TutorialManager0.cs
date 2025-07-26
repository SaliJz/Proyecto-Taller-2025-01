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

    [SerializeField] private TutorialSceneRuntime tutorialSceneRuntime=new TutorialSceneRuntime();

    public int currentDialogueIndex = 0;

    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] public List<DialogueData> listDialogueData = new List<DialogueData>();
    [SerializeField] private List<MonoBehaviour> playerScriptsToDisable;
    [SerializeField] private GameObject crossHairObject;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private List<CinemachineVirtualCamera> listVirtualCameras;
    [SerializeField] private FadeTransition fadeTransition;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GameObject weaponIcon;
    [SerializeField] private GameObject abilityIcon;
    [SerializeField] private GameObject HUD;
    public GameObject wallHologram;
    [SerializeField] private List<GameObject> supplyBox;
    [SerializeField] private List<GameObject> rifleAndShotgun;
    [SerializeField] private GameObject secondWaveGlitch;
    [SerializeField] private List<GameObject> blackListShopUI;
    //[SerializeField] private GameObject buyButton;
    public List<SpriteJumpToUI> spriteJumpToUIs;

    private int lastIndex = -1;

    private GameObject player;

    public bool hasConfirmedDialogueAdvance = false;
    private bool isDetectingSpace = false;
    private bool isDetectingWASD = false;
    private bool isInTransition;
    public event Action onConfirmAdvance;
    private bool isPlayerDisabled;

    public bool IsDialoguePlaying => GetCurrentDialogueData().isActive;

    [SerializeField] public Vector3 targetPosition1;
    [SerializeField] public Vector3 targetPosition2;
    [SerializeField] private float arriveSpeed;

    private bool hasActivatedSupplyBoxes = false;
    private bool hasActivatedRifleAndShotgun = false;
    private bool hasBlackListBeenClosed = false;

    private Weapon gunWeapon;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); 

        player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (Instance == null) Instance = this;

        if (tutorialSceneRuntime.tutorialSceneData == null)
        {
            Debug.LogError("tutorialSceneRuntime.tutorialSceneData' no está asignado en el Inspector.");
            return;
        }

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

    private void Update()
    {
        if (currentDialogueIndex == 1)
        {
            IsWASDPressed(); 
        }

        if (currentDialogueIndex == 9)
        {
            CheckPress123OurScroll();
            if (!hasActivatedRifleAndShotgun)
            {
                ActiveRifleAndShotgun();
                hasActivatedRifleAndShotgun = true;
            }
        }

        if (currentDialogueIndex == 10 && !hasActivatedSupplyBoxes)
        {
            foreach (GameObject box in supplyBox)
            {
                if (box != null && !box.activeSelf)
                {
                    box.SetActive(true);
                }
            }
            hasActivatedSupplyBoxes = true;
        }

    }
    void CheckPress123OurScroll()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetAxis("Mouse ScrollWheel") > 0f || 
            Input.GetAxis("Mouse ScrollWheel") < 0f)   
        {
            spriteJumpToUIs[4].gameObject.GetComponent<SpriteRenderer>().enabled = false;
            spriteJumpToUIs[4].ejecutarAnimacion = true;
            Destroy(spriteJumpToUIs[4].gameObject, 5f);
            ActivateSecondWaveGlitch();
            ConfirmAdvance();      
            //StartCoroutine(WaitNextDialogue(5));
        }
    }
    public void ScenarioActivationCheckerByZones()
    {
        if (GetCurrentDialogueData().activationType == ActivationType.ByZona)
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

    public void ActivateCurrentScenario()
    {
        if (GetCurrentDialogueData().isActive) return;

        GetCurrentDialogueData().isActive = true;
        StartCoroutine(PlayScenarioSequence());
    }

    private IEnumerator PlayScenarioSequence()
    {
        tutorialSceneRuntime.onSceneStart?.Invoke();

        foreach (var dialogue in tutorialSceneRuntime.tutorialSceneData.dialogues)
        {
            hasConfirmedDialogueAdvance = false;

            if (dialogue.dialogueText != null)
            {
                SetTextUI(dialogue.dialogueText);
            }

            yield return StartCoroutine(WaitForConfirmation());

            GetCurrentDialogueData().isActive = false;
            IncreaseDialogueIndex();
        }
    }

    private IEnumerator WaitForConfirmation()
    {
        hasConfirmedDialogueAdvance = false;
        onConfirmAdvance += Confirmed;

        if (currentDialogueIndex == 3)
        {
            EnabledPlayerDash();
        }

        if (currentDialogueIndex == 11)
        {
            dialogueTextUI.transform.parent.gameObject.SetActive(false);
        }
        

        yield return new WaitUntil(() => hasConfirmedDialogueAdvance);
        onConfirmAdvance -= Confirmed;
    }

    private void Confirmed()
    {
        hasConfirmedDialogueAdvance = true;
    }

    private void SetTextUI(string text)
    {
        dialogueTextUI.text = text;
    }

    void IncreaseDialogueIndex()
    {
        currentDialogueIndex += 1;
    }

    public DialogueData GetCurrentDialogueData()
    {
        return listDialogueData[currentDialogueIndex];
    }

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
            spriteJumpToUIs[0].gameObject.SetActive(true);
            onConfirmAdvance?.Invoke();                 
        }
    }

    private void IsWASDPressed()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            spriteJumpToUIs[1].gameObject.SetActive(true);
            onConfirmAdvance?.Invoke();
            spriteJumpToUIs[0].ejecutarAnimacion = true;
            spriteJumpToUIs[0].GetComponent<SpriteRenderer>().enabled = false;            
        }
    }
    public IEnumerator ActivateTransitionBetweenTwoCameras(int camera1, float time1, int camera2, float time2)
    {
        isInTransition=true;
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
        }
        DisablePlayerScriptsForCameraTransition();
        ActivateFadeOutOnCameraSwitch();
        yield return new WaitUntil(() => fadeTransition.isFadeIn == false);
        SelectCameraToRender(camera1);
        yield return new WaitForSecondsRealtime(time1);
        ActivateFadeOutOnCameraSwitch();
        yield return new WaitUntil(() => fadeTransition.isFadeIn == false);
        SelectCameraToRender(camera2);
        yield return new WaitForSecondsRealtime(time2);
        ReturnCamerasToDefault();

       

        Debug.Log("Valor de currentDialogueIndex en ActivateTransitionBetweenTwoCameras: " + currentDialogueIndex);

        EnablePlayerScriptsAfterCameraTransition();
        isInTransition = false;
    }
    private void SelectCameraToRender(int indexCamera)
    {
        ReturnCamerasToDefault();
        listVirtualCameras[indexCamera].Priority = 3;
    }
    private void ReturnCamerasToDefault()
    {
        foreach (CinemachineVirtualCamera camera in listVirtualCameras)
        {
            if (camera != listVirtualCameras[0])
            {
                camera.Priority = 1;
            }
        }
    }
    public void DisablePlayerScriptsForCameraTransition()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        crossHairObject.SetActive(false);

        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            script.enabled = false;
        }
    }

    void EnablePlayerScriptsAfterCameraTransition()
    {
        crossHairObject.SetActive(true);

        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (currentDialogueIndex < 3 && !(script is PlayerDash))
            {
                script.enabled = true;
            }
            else if (currentDialogueIndex >= 3)
            {
                script.enabled = true;
            }
        }
    }

    public void ActivateFadeOutOnCameraSwitch()
    {
        fadeTransition.fadeDuration = 0.2f;
        fadeTransition.delayBeforeFade = 0;
        fadeTransition.StartCoroutine(fadeTransition.FadeInOut(1.5f));
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

    public void ActiveHUD()
    {
        HUD.SetActive(true);
    }
    public void ActiveGun()
    {
        //if (gun != null) gun.SetActive(true);
        if (weaponManager != null) weaponManager.CanEquipFirstWeapon = true;
        if (weaponManager != null) weaponManager.EquipWeaponInstant(0);
        if (weaponIcon != null) weaponIcon.SetActive(true);


        if (gunWeapon != null)
        {
            HUDManager.Instance.UpdateAmmo(0, gunWeapon.CurrentAmmo, gunWeapon.TotalAmmo);
            HUDManager.Instance.UpdateWeaponIcon(gunWeapon.Stats.weaponIcon);
            HUDManager.Instance.UpdateWeaponName(gunWeapon.Stats.weaponName);
        }
        else
        {
            Debug.LogWarning("[tutorialSceneController] El componente Weapon no está asignado al objeto gun.");
        }
    }

    public IEnumerator WaitNextDialogue(float time)
    {
        yield return new WaitForSeconds(time);
        ConfirmAdvance();
    }

    public void ActivateSecondWaveGlitch()
    {
        if(secondWaveGlitch.activeSelf) return;
        secondWaveGlitch.SetActive(true);
    }

    public void ActiveRifleAndShotgun()
    {
        weaponManager.canChangeWeapon=true;
    }

    public void ActiveBlackList(int index)
    {

        if (hasBlackListBeenClosed)
        {
            return;
        }

        if (index == lastIndex)
        {
            return;
        }

        for (int i = 0; i < blackListShopUI.Count; i++)
        {
            if (blackListShopUI[i] != null)
                blackListShopUI[i].SetActive(false);
        }
        if (index == 6)
        {
            hasBlackListBeenClosed = true;
            //buyButton.SetActive(true);
            lastIndex = index;
            return;
        }

        if (index == 4 || index == 5)
        {
            if (index < blackListShopUI.Count)
                blackListShopUI[index].SetActive(true);

            if (6 < blackListShopUI.Count)
                blackListShopUI[6].SetActive(true); 
        }
        else
        {
            if (index < blackListShopUI.Count)
                blackListShopUI[index].SetActive(true);
        }

        //buyButton.SetActive(index != 4 && index != 5);
        lastIndex = index;
    }




    //public IEnumerator MovePlayerToPlatformCenter(Vector3 target)
    //{
    //    Vector3 targetXZ = new Vector3(target.x, player.transform.position.y, target.z);

    //    while (Vector3.Distance(player.transform.position, targetXZ) > 0.01f)
    //    {
    //        player.transform.position = Vector3.MoveTowards(player.transform.position, targetXZ, arriveSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //}
}
