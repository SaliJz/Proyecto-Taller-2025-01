using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageIdleState : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerAnimatorController.Instance?.SetIdleProcedural(true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerAnimatorController.Instance?.SetIdleProcedural(false);
    }
}