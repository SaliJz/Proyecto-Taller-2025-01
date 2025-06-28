using UnityEngine;

public class Glitch_Cinematic : MonoBehaviour
{
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] float speed = 2f;

    [SerializeField] bool isMoving = false;

    [SerializeField] private Animator animator;
    [SerializeField] bool animatorIsActive;
    void Update()
    {
        if (isMoving)
        {
            MoveToB();
        }

        if (animatorIsActive)
        {
            animatorIsActive = false;
            animator.SetTrigger("isActive");
        }
    }

    public void StartMovingToB()
    {
        transform.position = pointA.position; // Opcional: empieza desde A
        isMoving = true;
    }

    void MoveToB()
    {
        transform.position = Vector3.MoveTowards(transform.position, pointB.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, pointB.position) < 0.01f)
        {
            isMoving = false;
        }
    }
}
