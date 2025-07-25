using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Destroy_VFX : MonoBehaviour
{
    public float Destroy_Delay;
    public VisualEffect vfx;
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.playRate = 1.5f; 
        StartCoroutine(delay());
    }
   
    IEnumerator delay()
    {
        yield return new WaitForSeconds(Destroy_Delay);

        Destroy(gameObject);
    }
}
