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
        portalAnimator.SetTrigger("isClose");
    }
}
