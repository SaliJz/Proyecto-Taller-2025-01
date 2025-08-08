using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackableSurface : MonoBehaviour, IHackable
{
    public enum WallSide { Forward, Back, Left, Right, Up, Down }

    [Header("Configuración de la pared adaptable")]
    [Tooltip("El Prefab de la pared adaptable que se va a instanciar.")]
    [SerializeField] private GameObject adaptiveWallPrefab;

    [Tooltip("La distancia máxima a la que se buscará una pared opuesta.")]
    [SerializeField] private float maxWallDistance = 50f;

    [Tooltip("La altura y el grosor fijos de la pared que se creará.")]
    [SerializeField] private float wallHeight = 5f;
    [SerializeField] private float wallThickness = 0.5f;

    [Header("Visualización y selección de lado")]
    [Tooltip("Selecciona el lado desde el cual se generará el muro.")]
    [SerializeField] private WallSide wallSide = WallSide.Forward;

    private Vector3 GetWallDirection()
    {
        switch (wallSide)
        {
            case WallSide.Forward: return transform.forward;
            case WallSide.Back: return -transform.forward;
            case WallSide.Left: return -transform.right;
            case WallSide.Right: return transform.right;
            case WallSide.Up: return transform.up;
            case WallSide.Down: return -transform.up;
            default: return transform.forward;
        }
    }

    private Vector3 GetFacePoint()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return transform.position;

        Vector3 dir = GetWallDirection();
        Vector3 extents = col.bounds.extents;
        Vector3 localOffset = Vector3.zero;

        // Calcula el offset local según la dirección
        if (dir == transform.forward) localOffset = new Vector3(0, 0, extents.z);
        else if (dir == -transform.forward) localOffset = new Vector3(0, 0, -extents.z);
        else if (dir == transform.right) localOffset = new Vector3(extents.x, 0, 0);
        else if (dir == -transform.right) localOffset = new Vector3(-extents.x, 0, 0);
        else if (dir == transform.up) localOffset = new Vector3(0, extents.y, 0);
        else if (dir == -transform.up) localOffset = new Vector3(0, -extents.y, 0);

        // Transforma el offset local a mundo
        return col.bounds.center + transform.rotation * localOffset;
    }

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (adaptiveWallPrefab == null)
        {
            Debug.LogError("Adaptive Wall Prefab no está asignado en " + gameObject.name);
            return;
        }

        Vector3 wallDirection = GetWallDirection();
        Vector3 startPoint = GetFacePoint();

        RaycastHit hitInfo;
        if (Physics.Raycast(startPoint + wallDirection * 0.05f, wallDirection, out hitInfo, maxWallDistance))
        {
            Vector3 endPoint = hitInfo.point;

            // 1. Calcular la posición central para el nuevo muro.
            Vector3 centerPosition = (startPoint + endPoint) / 2f;

            // 2. Calcular la distancia para la escala del muro.
            float wallLength = Vector3.Distance(startPoint, endPoint);

            // 3. Calcular la rotación para que el muro mire de un punto al otro.
            Quaternion wallRotation = Quaternion.LookRotation(endPoint - startPoint);

            // 4. Instanciar el muro.
            GameObject wallInstance = Instantiate(adaptiveWallPrefab, centerPosition, wallRotation);

            // 5. Escalar el muro para que encaje perfectamente.
            // El eje Z del objeto se alineará con la longitud.
            wallInstance.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);

            // 6. Inicializar el muro para que se destruya después de la duración.
            wallInstance.GetComponent<EnergyWall>()?.Initialize(duration);

            Debug.Log("Pared adaptable creada con una longitud de " + wallLength);
        }
        else
        {
            Debug.Log("No se encontró una pared opuesta para crear la barrera.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 wallDirection = GetWallDirection();
        Vector3 startPoint = GetFacePoint();
        Gizmos.DrawLine(startPoint, startPoint + wallDirection * maxWallDistance);
        Gizmos.DrawWireCube(startPoint + wallDirection * (maxWallDistance / 2f), new Vector3(wallThickness, wallHeight, maxWallDistance));
    }
}