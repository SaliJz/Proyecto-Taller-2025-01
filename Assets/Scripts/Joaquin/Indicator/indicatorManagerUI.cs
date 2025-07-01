using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class indicatorManagerUI : MonoBehaviour
{
    #region Singleton

    public static indicatorManagerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    #endregion

    [SerializeField] private GameObject prefabIndicatorUI;
    [SerializeField] private Transform panelIndicators;

    private Dictionary<TargetIndicator, GameObject> indicators = new Dictionary<TargetIndicator, GameObject>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void AddTarget(TargetIndicator target)
    {
        if (!indicators.ContainsKey(target))
        {
            GameObject newIndicator = Instantiate(prefabIndicatorUI, panelIndicators);
            indicators[target] = newIndicator;
        }
    }

    public void RemoveTarget(TargetIndicator target)
    {
        if (indicators.TryGetValue(target, out GameObject indicator))
        {
            Destroy(indicator);
            indicators.Remove(target);
        }
    }

    private void LateUpdate()
    {
        foreach (var par in indicators)
        {
            TargetIndicator target = par.Key;
            GameObject indicatorUI = par.Value;

            // 1. Convertir la posici�n del mundo a posici�n de pantalla
            Vector3 cameraPos = mainCamera.WorldToScreenPoint(target.transform.position);

            // 2. Comprobar si est� dentro del frustum de la c�mara (visible)
            if (cameraPos.z > 0 &&
                cameraPos.x > 0 && cameraPos.x < Screen.width &&
                cameraPos.y > 0 && cameraPos.y < Screen.height)
            {
                // El objetivo est� en pantalla. Ocultamos el indicador de borde.
                indicatorUI.SetActive(false);
            }
            else
            {
                // El objetivo est� fuera de pantalla. Mostramos y posicionamos el indicador.
                indicatorUI.SetActive(true);

                // Si el z es negativo, el objeto est� detr�s de nosotros.
                // Invertimos la posici�n para que el indicador aparezca en el borde correcto.
                if (cameraPos.z < 0)
                {
                    cameraPos *= -1;
                }

                // Centramos la posici�n y encontramos la direcci�n
                Vector3 center = new Vector3(Screen.width, Screen.height, 0) / 2;
                cameraPos -= center;

                // Calculamos el �ngulo para rotar el indicador (opcional)
                float angle = Mathf.Atan2(cameraPos.y, cameraPos.x) * Mathf.Rad2Deg;
                indicatorUI.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Sujetamos la posici�n a los bordes de la pantalla (clamping)
                float edgeX = Screen.width / 2 * 0.9f;  // 90% del ancho
                float edgeY = Screen.height / 2 * 0.9f; // 90% del alto

                cameraPos.x = Mathf.Clamp(cameraPos.x, -edgeX, edgeX);
                cameraPos.y = Mathf.Clamp(cameraPos.y, -edgeY, edgeY);

                indicatorUI.transform.position = center + cameraPos;
            }
        }
    }
}
