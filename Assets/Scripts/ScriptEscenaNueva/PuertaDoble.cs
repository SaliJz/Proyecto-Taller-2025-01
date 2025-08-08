using UnityEngine;
using UnityEngine.Events;

public class PuertaDoble : MonoBehaviour
{
    [Header("Puerta 1")]
    [SerializeField] Transform Puerta1;
    [SerializeField] Transform Destination1;


    [Header("Puerta 2")]
    [SerializeField] Transform Puerta2;
    [SerializeField] Transform Destination2;


    [Header("Cambio de emission")]
    [SerializeField] Renderer[] targetRenderers;
    [ColorUsage(true, true)]
    public Color newEmissionColor = Color.white;
    [SerializeField] Material[] targetMaterial;


    [Header("Variables")]
    [SerializeField] Transform Player;
    [SerializeField] Collider PassCollider;
    [SerializeField] Collider Barrier;
    [SerializeField] float openVel;

    Vector3 InitialPos1;
    Vector3 InitialPos2;

    public bool IsPassed = false;

    public void IsPassedTrue()
    {
        IsPassed = true;
    }

    private void Start()
    {
        InitialPos1 = Puerta1.position;
        InitialPos2 = Puerta2.position;

        Barrier.enabled = false;

        targetMaterial = new Material[targetRenderers.Length];

        for(int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
            {
                targetMaterial[i] = targetRenderers[i].material;
                targetMaterial[i].EnableKeyword("_EMISSION");

            }
        }
    }
    private void Update()
    {

        if (!IsPassed)
        {
            if (Vector3.Distance(Player.position, transform.position) < 8)
            {
                OpenDoors();
            }
            else
            {
                CloseDoors();
            }
        }
        else
        {
            Barrier.enabled = true;
            CloseDoors();
        }

    }

    void OpenDoors()
    {
        Vector3 NewDestination1 = new Vector3(Destination1.position.x, Puerta1.position.y, Destination1.position.z);
        Vector3 NewDestination2 = new Vector3(Destination2.position.x, Puerta2.position.y, Destination2.position.z);


        Puerta1.position = Vector3.Lerp(Puerta1.position, NewDestination1, openVel);
        Puerta2.position = Vector3.Lerp(Puerta2.position, NewDestination2, openVel);
    }

    void CloseDoors()
    {
        Puerta1.position = Vector3.Lerp(Puerta1.position, InitialPos1, openVel);
        Puerta2.position = Vector3.Lerp(Puerta2.position, InitialPos2, openVel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Paso la puerta");
            IsPassed = true;
            Barrier.enabled = true;
            PassCollider.enabled = true;

            foreach (Material mat in targetMaterial)
            {
                if(mat != null)
                {
                    mat.SetColor("_EmissionColor", newEmissionColor);
                }
            }
        }
    }

    public float GetOpenVelocity()
    {
        return openVel;
    }

    public void SetOpenVelocity(float newOpenVel)
    {
        openVel = newOpenVel;
    }
}
