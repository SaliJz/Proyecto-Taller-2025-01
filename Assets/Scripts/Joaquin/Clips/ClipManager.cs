using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClipManager : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    private bool clipPauseGame = false;

    public bool ClipPauseGame
    {
        get { return clipPauseGame; }
        set { clipPauseGame = value; }
    }

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(CloseClipManager);
        else Debug.LogError("closeButton no está asignado en el Inspector.");
    }

    private void Start()
    {
        clipPauseGame = true; // Enable clip pause game mode
        StartCoroutine(ForcePauseAndCursor());
    }

    private IEnumerator ForcePauseAndCursor()
    {
        yield return null; // Espera un frame para que otros scripts terminen
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseClipManager()
    {
        Debug.Log("Closing Clip Manager");

        ClipPauseGame = false; // Disable clip pause game mode
        Time.timeScale = 1f; // Resume the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false); // Hide the ClipManager UI
    }
}
