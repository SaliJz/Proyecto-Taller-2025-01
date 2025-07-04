using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LOGIN1 : MonoBehaviour
{
    [SerializeField] private TMP_InputField NAME;
    [SerializeField] private TMP_InputField PASSWORD;
    [SerializeField] private Button LOGINN;


    private LOGIN controller;

    private void Awake()
    {
        controller = GetComponent<LOGIN>();
    }

    void Start()
    {
        LOGINN.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        string player_name = NAME.text;
        string password = PASSWORD.text;
        controller.Execute(player_name, password);
    }

}