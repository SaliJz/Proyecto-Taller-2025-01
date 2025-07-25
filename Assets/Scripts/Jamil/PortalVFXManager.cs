using System.Collections.Generic;
using UnityEngine;

public class PortalVFXManager : MonoBehaviour
{
    public static PortalVFXManager Instance { get; private set; }
    [SerializeField] private List<PortalVFXController> listPortalVFX;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void CloseAllPortals()
    {
        foreach(PortalVFXController portal in listPortalVFX)
        {
            if (portal != null)
            {
                portal.DeactivePortalAnimation();
            }
        }
    }

    public void ActiveAllPortals()
    {
        Debug.Log("Portal Activado");
        foreach (PortalVFXController portal in listPortalVFX)
        {
            if (portal != null)
            {
               portal.gameObject.SetActive(true);
            }
        }
    }
}
