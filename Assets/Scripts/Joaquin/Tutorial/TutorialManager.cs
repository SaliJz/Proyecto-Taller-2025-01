using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 


public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance
    { get; private set; }

    public List<SceneTutorial> scenes;
    public TextMeshProUGUI dialoguesText;
    public AudioSource voicesSource;

    public float speedText = 0.04f;
    private int currentScene = 0;

    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartScenarioByZone(int index)
    {
        StopAllCoroutines();
        StartCoroutine(StartScenario(index));
    }

    IEnumerator StartScenario(int index)
    {
        SceneTutorial escenario = scenes[index];
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        escenario.events.Invoke();

        foreach (DialogueData d in escenario.dialogues)
        {
            StartCoroutine(WriteText(d.dialogueText));
            if (d.dialogueVoice != null) voicesSource.PlayOneShot(d.dialogueVoice);
            yield return new WaitForSeconds(d.dialogueDuration);
        }

        currentScene++;

        if (currentScene < scenes.Count) StartCoroutine(StartScenario(currentScene));
    }

    IEnumerator WriteText(string text)
    {
        dialoguesText.text = "";
        foreach (char c in text)
        {
            dialoguesText.text += c;
            yield return new WaitForSeconds(speedText);
        }
    }
}
