using UnityEngine;
using UnityEngine.UI;

public class DELETEusuarioView : MonoBehaviour
{
    [SerializeField] private Button DELETE;

    private DELETEusuario controller;

    private void Awake()
    {
        controller = GetComponent<DELETEusuario>();
    }

    void Start()
    {
        DELETE.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        controller.BorrarUsuario(LOGIN.loginmode.data);
    }
}
