using UnityEngine;

public class PortalVFXController : MonoBehaviour
{
    private Animator portalAnimator;
    private void Start()
    {
        portalAnimator=GetComponent<Animator>();
    }

    public void DeactivePortalAnimation()
    {
        if (portalAnimator != null)
            portalAnimator.SetTrigger("isClose");
        
            if (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                Destroy(child.gameObject);
            }
        
    }
}
