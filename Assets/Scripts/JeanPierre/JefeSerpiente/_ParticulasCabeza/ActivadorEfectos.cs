// ActivadorEfectos.cs
using System.Collections;
using UnityEngine;

public class ActivadorEfectos : MonoBehaviour
{
    [Tooltip("Retraso en segundos antes de asignar el EfectosController")]
    public float retrasoAsignacion = 2f;

    public bool activar;
    private bool prevState;
    private EfectosController ctrl;

    private IEnumerator Start()
    {
        // Espera a que SnakeController y la cabeza existan
        var snake = FindObjectOfType<SnakeController>();
        while (snake == null || snake.Segmentos.Count == 0)
        {
            yield return null;
            snake = FindObjectOfType<SnakeController>();
        }

        // Espera el tiempo definido en la variable
        yield return new WaitForSeconds(retrasoAsignacion);

        // Asigna el controlador de efectos tras el retraso
        var head = snake.Segmentos[0];
        ctrl = head.GetComponent<EfectosController>();
        prevState = !activar;
    }

    private void Update()
    {
        if (ctrl == null || activar == prevState) return;
        ctrl.SetActive(activar);
        prevState = activar;
    }
}
