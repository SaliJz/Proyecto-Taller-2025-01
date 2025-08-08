using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour, IHackable, ISlowable
{
    private bool isHacked = false;
    private bool pendingRestoreRotation = false;
    private float originalRotationSpeed;
    private float affectedRotationSpeed;
    [SerializeField] private Turret turret;

    private void Awake()
    {
        turret = gameObject.GetComponent<Turret>();
        if (turret == null) Debug.LogWarning("No se encontró el componente Turret en " + gameObject.name);
    }

    private void Update()
    {
        if (turret == null)
        {
            Debug.LogError("No se encontró el componente Turret en " + gameObject.name + ". TurretController no funcionará correctamente.");
            enabled = false;
            return;
        }

        if (originalRotationSpeed == 0)
        {
            float speed = turret.GetRotarionSpeed();
            if (speed == 0)
            {
                return;
            }
            originalRotationSpeed = speed;
        }
    }

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (isHacked) return;
        StartCoroutine(HackRoutine(duration));
    }

    private IEnumerator HackRoutine(float duration)
    {
        isHacked = true;
        Debug.Log("Torreta desactivada por " + duration + " segundos.");
        turret.enabled = false;

        yield return new WaitForSeconds(duration);

        turret.enabled = true;
        isHacked = false;
        Debug.Log("Torreta reactivada.");

        if (pendingRestoreRotation)
        {
            turret.SetRotationSpeed(originalRotationSpeed);
            pendingRestoreRotation = false;
            Debug.Log("Velocidad de torreta restaurada tras hack.");
        }
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        affectedRotationSpeed = originalRotationSpeed * multiplier;
        turret.SetRotationSpeed(affectedRotationSpeed);
        Debug.Log("Velocidad de torreta reducida.");

        yield return new WaitForSeconds(duration);

        affectedRotationSpeed = originalRotationSpeed;
        turret.SetRotationSpeed(originalRotationSpeed);
        Debug.Log("Velocidad de torreta restaurada.");
    }
}
