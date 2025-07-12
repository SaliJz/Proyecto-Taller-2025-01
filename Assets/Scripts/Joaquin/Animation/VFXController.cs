using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    [System.Serializable]
    private class VFXEntry
    {
        public string id;
        public GameObject vfxObject;
    }

    [Header("Lista de VFX asignados")]
    [SerializeField] private List<VFXEntry> vfxList;

    private Dictionary<string, GameObject> vfxDict;

    private void Awake()
    {
        vfxDict = new Dictionary<string, GameObject>();
        foreach (var entry in vfxList)
        {
            if (!vfxDict.ContainsKey(entry.id)) vfxDict.Add(entry.id, entry.vfxObject);
        }
    }

    public void ActivateVFX(string id)
    {
        foreach (var pair in vfxDict)
        {
            pair.Value.SetActive(pair.Key == id);
        }
    }

    public void DeactivateAll()
    {
        foreach (var pair in vfxDict)
        {
            pair.Value.SetActive(false);
        }
    }
}
