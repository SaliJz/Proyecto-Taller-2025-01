using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    public static PlayerAnimatorController Instance { get; private set; }

    [Header("Referencias")]
    [Tooltip("El Animator que controla las animaciones de cambio de arma.")]
    [SerializeField] private Animator weaponAnimator;
    [Tooltip("El Animator que controla las animaciones de uso de habilidad.")]
    [SerializeField] private Animator abilityAnimator;

    [Header("Scripts de Idle Procedural")]
    [Tooltip("El script IdleSuave en el objeto de la mano de armas.")]
    [SerializeField] private ArmsIdle weaponIdleScript;
    [Tooltip("El script IdleSuave en el objeto de la mano de habilidades.")]
    [SerializeField] private ArmsIdle abilityIdleScript;

    private readonly int hashSwitchWeapon = Animator.StringToHash("SwitchWeapon");
    private readonly int hashWeaponID = Animator.StringToHash("WeaponID");
    private readonly int hashFireAbility = Animator.StringToHash("FireAbility");
    private readonly int hashAbilityID = Animator.StringToHash("AbilityID");
    private readonly int hashDash = Animator.StringToHash("Dash");

    private WeaponManager weaponManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
    }

    private void Update()
    {
        if (weaponManager == null) return;
    }

    public void PlaySwitchWeaponAnim(int weaponIndex) // Acepta un ID
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetFloat(hashWeaponID, (float)weaponIndex);
            weaponAnimator.SetTrigger(hashSwitchWeapon);
        }
    }

    public void PlayFireAbilityAnim(int abilityIndex) // Acepta un ID
    {
        if (abilityAnimator != null)
        {
            abilityAnimator.SetFloat(hashAbilityID, (float)abilityIndex);
            abilityAnimator.SetTrigger(hashFireAbility);
        }
    }

    public void PlayDashAnim()
    {
        if (weaponAnimator != null) weaponAnimator.SetTrigger(hashDash);
        if (abilityAnimator != null) abilityAnimator.SetTrigger(hashDash);
    }

    public void SetIdleProcedural(bool isActive)
    {
        if (weaponIdleScript != null) weaponIdleScript.enabled = isActive;
        if (abilityIdleScript != null) abilityIdleScript.enabled = isActive;
    }
}
