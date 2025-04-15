using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAITest : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float life = 100f;
    [SerializeField] private int fragments = 50; // Fragmentos de información que suelta el enemigo
    private float currentSpeed;

    private void Start()
    {
        currentSpeed = baseSpeed;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(multiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        currentSpeed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        life -= dmg; // Ejemplo de daño fijo
        if (life <= 0)
        {
            Destroy(gameObject); // Destruye el enemigo al morir
            HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragmentos de información
        }
    }

    private void Update()
    {
        // ejemplo simple
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
