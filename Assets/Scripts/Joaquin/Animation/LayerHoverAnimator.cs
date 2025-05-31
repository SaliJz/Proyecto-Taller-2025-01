using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LayerHoverAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private float delayedTime = 10f;
    private bool hasHighlighted = false;
    private Coroutine exitCoroutine;

    private void Awake()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hasHighlighted)
        {
            animator.SetBool("IsHighlighted", true);
            hasHighlighted = true;
        }

        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hasHighlighted && exitCoroutine == null)
        {
            exitCoroutine = StartCoroutine(DelayedExit());
        }
    }

    private IEnumerator DelayedExit()
    {
        yield return new WaitForSeconds(delayedTime);
        animator.SetBool("IsHighlighted", false);
        hasHighlighted = false;
        exitCoroutine = null;
    }
}
