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

    [SerializeField] private List<RuntimeSceneData> scenes;
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

    public void StartScenarioByKills(int index)
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByKills)
        {
            killsByScenario[index]++;
            Debug.Log($"Kills para escenario {index}: {killsByScenario[index]}/{scenes[index].sceneData.necessarykills}");
            if (killsByScenario[index] >= scenes[index].sceneData.necessarykills)
            {
                StartScenario(index);
            }
        }
    }

    public void StartScenarioByTime(int index, float time)
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ForTime)
        {
            StartCoroutine(WaitForTime(index, time));
        }
    }

    public void StartScenarioByManual(int index)
    {
        if (!scenes[index].isActive && scenes[index].sceneData.activationType == ActivationType.ByEventoManual)
        {
            StartScenario(index);
        }
    }

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

    private IEnumerator WaitForTime(int index, float time)
    {
        yield return new WaitForSeconds(time);
        if (!scenes[index].isActive)
        {
            StartScenario(index);
        }
    }

    private void StartScenario(int index)
    {
        if (scenes[index].isActive) return;
        scenes[index].isActive = true;
        StopAllCoroutines();
        StartCoroutine(RunScenario(index));
    }

    private IEnumerator RunScenario(int index)
    {
        var runtimeScene = scenes[index];
        runtimeScene.onSceneStart?.Invoke();

        Debug.Log($"RUN SCENARIO {index} Di�logos: {runtimeScene.sceneData.dialogues.Count}");


        foreach (var dialogue in runtimeScene.sceneData.dialogues)
        {
            if (dialogue.dialogueVoice) voicesSource.PlayOneShot(dialogue.dialogueVoice);
            yield return StartCoroutine(TypeText(dialogue.dialogueText));
        }

        OnScenarioEnded(index);
    }

    private void OnScenarioEnded(int finishedIndex)
    {
        // Busca el siguiente escenario (puedes ajustar la l�gica si hay saltos)
        int nextIndex = finishedIndex + 1;
        if (nextIndex < scenes.Count)
        {
            var nextScene = scenes[nextIndex];
            if (nextScene.sceneData.activationType == ActivationType.ForTime && !nextScene.isActive)
            {
                // Inicia el temporizador solo ahora
                StartScenarioByTime(nextIndex, nextScene.sceneData.necessaryTime);
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