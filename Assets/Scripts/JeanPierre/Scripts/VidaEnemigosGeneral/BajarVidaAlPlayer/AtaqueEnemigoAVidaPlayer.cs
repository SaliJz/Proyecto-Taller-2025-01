using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueEnemigoAVidaPlayer : MonoBehaviour
{
    [SerializeField] private int damageAmount = 15;
    [SerializeField] private float damageInterval = 1.5f;

    [Header("Variantes de Ataque")]
    [SerializeField] private GameObject baseAttackObject;
    [SerializeField] private bool useAttackVariants = false;
    [SerializeField] private List<GameObject> attackVariants;

    private Animator animator;
    private Coroutine damageCoroutine;
    private float lastStayTime;

    private void Awake()
    {
        GameObject activeAttackObject;
        if (useAttackVariants && attackVariants.Count > 0)
        {
            if (baseAttackObject != null)
            {
                baseAttackObject.SetActive(false);
            }

            foreach (var variant in attackVariants)
            {
                if (variant != null)
                {
                    variant.SetActive(false);
                }
            }

            int randomIndex = Random.Range(0, attackVariants.Count);
            activeAttackObject = attackVariants[randomIndex];
            if (activeAttackObject != null)
            {
                activeAttackObject.SetActive(true);
            }
        }
        else
        {
            activeAttackObject = baseAttackObject;
            if (activeAttackObject != null)
            {
                activeAttackObject.SetActive(true);
            }

            foreach (var variant in attackVariants)
            {
                if (variant != null)
                {
                    variant.SetActive(false);
                }
            }
        }

        if (activeAttackObject != null)
        {
            animator = activeAttackObject.GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && damageCoroutine == null)
            {
                lastStayTime = Time.time;
                damageCoroutine = StartCoroutine(DamagePlayerRoutine(playerHealth));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastStayTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
            if (animator != null)
            {
                animator.SetBool("Ataque", false);
            }
        }
    }

    private IEnumerator DamagePlayerRoutine(PlayerHealth playerHealth)
    {
        while (true)
        {
            if (animator != null)
            {
                animator.SetBool("Ataque", true);
            }

            playerHealth.TakeDamage(damageAmount, transform.position);

            yield return new WaitForSeconds(damageInterval);

            if (animator != null)
            {
                animator.SetBool("Ataque", false);
            }

            if (Time.time - lastStayTime > damageInterval)
            {
                break;
            }
        }

        damageCoroutine = null;
    }
}