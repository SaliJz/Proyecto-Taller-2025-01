using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsertUsuarioView : MonoBehaviour
{
    [SerializeField] private TMP_InputField NAME;
    [SerializeField] private TMP_InputField PASSWORD;
    [SerializeField] private Button REGISTRER;
    [SerializeField] private TMP_Text textoUI;

    private InsertUsuario controller;

    private void Awake()
    {
        controller = GetComponent<InsertUsuario>();
        controller.OnMensajeRecibido += MostrarMensaje;
    }

    void Start()
    {
        REGISTRER.onClick.AddListener(OnButtonPressed);

        controller.OnMensajeRecibido += (mensaje) =>
        {
            Debug.Log("Mensaje recibido: " + mensaje);
            textoUI.text = mensaje; 
        };
    }

    private void OnButtonPressed()
    {
        string player_name = NAME.text;
        string password=PASSWORD.text;
        controller.Execute(player_name, password);
    }

    private void MostrarMensaje(string mensaje)
    {
        textoUI.text = mensaje;
    }
}
