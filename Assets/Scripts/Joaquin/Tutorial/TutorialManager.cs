using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; 


public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance
    { get; private set; }

    [System.Serializable]
    public class RuntimeSceneData
    {
        public SceneTutorial sceneData; 
        public UnityEvent onSceneStart;
        [HideInInspector] public bool isActive = false;
    }
     
    public List<RuntimeSceneData> scenes; //Ahora es publica
    public int currentIndex = 0; //Para saber en que indice estamos , me sirve para la escena 2 en adelante
    [SerializeField] private TextMeshProUGUI dialoguesText;
    [SerializeField] private AudioSource voicesSource;
    [SerializeField] private float speedText = 0.04f;
    

    private int[] killsByScenario;
    private int[] fragmentsByScenario;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        killsByScenario = new int[scenes.Count];
        fragmentsByScenario = new int[scenes.Count];
    }

    //void ChangeIndex(int index)
    //{
    //   currentIndex= index+1;
    //}

    //ESCENAS 0 Y 1
    public void StartScenarioByZone(int index)
    {
        if (index < 0 || index >= scenes.Count)
        {
            Debug.LogError($"[TutorialManager] Index fuera de rango: {index}. Total escenas: {scenes.Count}");
            return;
        }

        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByZona)
        {
            StartScenario(index);
        }
    }

    //ESCENAS 2 Y 3
    public void StartScenarioByKills(int index)   //Es activado por los glitchs al morir
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByKills)
        {
            killsByScenario[index]++;
            Debug.Log($"Kills para escenario {index}: {killsByScenario[index]}/{scenes[index].sceneData.necessarykills}");
            if (killsByScenario[index] >= scenes[index].sceneData.necessarykills)
            {
                StartScenario(index);
                //ChangeIndex(index);
                killsByScenario[index]=0;
                Debug.Log("Conseguiste las kills necesarias");
            }           

        }
    }

    //ESCENAS 4 , 9 Y 10
    public void StartScenarioByTime(int index, float time)
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ForTime)
        {
            StartCoroutine(WaitForTime(index, time));
        }
    }

    private IEnumerator WaitForTime(int index, float time)
    {
        yield return new WaitForSeconds(time);
        if (!scenes[index].isActive)
        {
            StartScenario(index);
        }
    }

    //ESCENAS 5, 6 Y 8
    public void StartScenarioByManual(int index)
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByEventoManual)
        {
            StartScenario(index);
        }
    }
    //ESCENA 7
    public void StartScenarioByFragments(int index) 
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByFragments)
        {
            fragmentsByScenario[index]++;
            Debug.Log($"Fragmentos para escenario {index}: {fragmentsByScenario[index]}/{scenes[index].sceneData.necessaryFragments}");
            if (fragmentsByScenario[index] >= scenes[index].sceneData.necessaryFragments)
            {
                StartScenario(index);
            }
        }
    }


    // DETIENE CORRUTINAS 
    private void StartScenario(int index)
    {
        Debug.Log("El indice actual es:" + index);
        if (scenes[index].isActive) return;
        scenes[index].isActive = true;
        StopAllCoroutines();
        StartCoroutine(RunScenario(index));
    }

    // ACTIVA TODAS LAS FUNCIONES ASOCIADAS AL ESCENARIO, Y REPRODUCE VOZ Y TEXOS
    private IEnumerator RunScenario(int index)
    {
        var runtimeScene = scenes[index];
        runtimeScene.onSceneStart?.Invoke();

        foreach (var dialogue in runtimeScene.sceneData.dialogues)
        {
            if (dialogue.dialogueVoice != null) voicesSource.PlayOneShot(dialogue.dialogueVoice);
            yield return StartCoroutine(TypeText(dialogue.dialogueText));
        }
       
        OnScenarioEnded(index); 
    }


    private void OnScenarioEnded(int finishedIndex)
    {
        currentIndex = finishedIndex + 1; 

        if (currentIndex < scenes.Count)
        {
            var nextScene = scenes[currentIndex];

            if (nextScene.sceneData.activationType == ActivationType.ForTime && !nextScene.isActive)
            {
                StartScenarioByTime(currentIndex, nextScene.sceneData.necessaryTime);
            }
        }
    }

    private IEnumerator TypeText(string text)
    {
        Debug.Log("TEXTO: " + text);

        dialoguesText.text = "";
        foreach (char characters in text)
        {
            dialoguesText.text += characters;
            yield return new WaitForSeconds(speedText);
        }
    }

}