using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlitchAbility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Glitch Settings")]
    [SerializeField] private GameObject glitchProjectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float cooldown = 12f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileSpeed = 50f;

    [Header("Spread")]
    [SerializeField] private float spreadIntensity;

    [Header("UI")]
    [SerializeField] private TMP_Text cooldownText;

    private bool canUse = true;
    private float currentCooldown = 0f;

    private void Start()
    {
        cooldownText.text = "Glitch: Ready";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUse)
        {
            FireGlitchShot();
        }

        if (!canUse)
        {
            currentCooldown -= Time.deltaTime;
            cooldownText.text = $"Glitch: {Mathf.Ceil(currentCooldown)}";

            if (currentCooldown <= 0f)
            {
                canUse = true;
                cooldownText.text = "Glitch: Ready";
            }
        }
    }

    private void FireGlitchShot()
    {
        Vector3 direction = CalculateDirectionAndSpread().normalized;
        GameObject projectile = Instantiate(glitchProjectile, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        canUse = false;
        currentCooldown = cooldown;
        projectile.GetComponent<Rigidbody>().AddForce(direction * projectileSpeed, ForceMode.Impulse);
        Destroy(projectile, projectileLifeTime);
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = targetPoint - projectileSpawnPoint.position;

        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }
}
