using System.Collections;
using System.Collections.Generic;
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

        GameObject portal = portalAnimator.gameObject;

        if (portal != null)
        {
            if (portal.transform.childCount > 0)
            {
                Transform child = portal.transform.GetChild(0);
                Destroy(child.gameObject);
            }
        }
    }
}
