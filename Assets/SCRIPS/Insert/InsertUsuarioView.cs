using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsertUsuarioView : MonoBehaviour
{
    [SerializeField] private TMP_InputField NAME;
    [SerializeField] private TMP_InputField PASSWORD;
    [SerializeField] private Button REGISTRER;


    private InsertUsuario controller;

    private void Awake()
    {
        controller = GetComponent<InsertUsuario>();
    }

    void Start()
    {
        REGISTRER.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        string player_name = NAME.text;
        string password=PASSWORD.text;
        controller.Execute(player_name, password);
    }

}
