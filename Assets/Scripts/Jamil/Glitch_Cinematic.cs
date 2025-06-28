using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_Cinematic : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    //[SerializeField] private string animationName ="Glitch_Cinematic"; 

    public bool isMoving = false;
    private bool activateAnimator = false;

    void Update()
    {
        if (isMoving)
        {
            MoveToB();

            if (!activateAnimator)
            {
                ActivateAnimator();
                activateAnimator = true;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Glitch_Cinematic") && stateInfo.normalizedTime >= 0.8f)
            {
                isMoving = false;
            }
        }
    }

    public void StartMovingToB()
    {
        //transform.position = pointA.position;
        isMoving = true;
        activateAnimator = false; 
    }

    private void MoveToB()
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(pointB.position.x, currentPosition.y, pointB.position.z);

        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
    }

    public void ActivateAnimator()
    {
        if (animator != null)
        {
            animator.SetTrigger("isActive");
        }
    }
}
