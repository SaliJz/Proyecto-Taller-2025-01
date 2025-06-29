using UnityEngine;
using UnityEngine.VFX;

public class EfectosController : MonoBehaviour
{
    private ParticleSystem[] ps;
    private VisualEffect[] vfx;

    void Awake()
    {
        ps = GetComponentsInChildren<ParticleSystem>(true);
        vfx = GetComponentsInChildren<VisualEffect>(true);
    }

    public void SetActive(bool on)
    {
        foreach (var p in ps) if (on) p.Play(); else p.Stop();
        foreach (var v in vfx) if (on) v.Play(); else v.Stop();
    }
}
