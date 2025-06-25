using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;


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
    [SerializeField] private float textTypingSpeed = 0.04f;
    [SerializeField] private TutorialSceneController tutorialSceneController;
    [SerializeField] private GameObject FragmentOfTheTutorial;
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
        StopAllCoroutines();
        StartCoroutine(PlayScenarioSequence());
    }

    private IEnumerator PlayScenarioSequence()
    {
        var runtimeScene = GetTutorialCurrentScene();
        runtimeScene.onSceneStart?.Invoke();

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

    private IEnumerator AnimateTextTyping(string text)
    {
        //Debug.Log("TEXTO: " + text);
        dialogueTextUI.text = "";

        foreach (char character in text)
        {
            dialogueTextUI.text += character;
            yield return new WaitForSecondsRealtime(textTypingSpeed);
        }
    }
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
}
