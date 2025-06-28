
using UnityEngine;
using UnityEngine.AI;


public class TutorialSceneController : MonoBehaviour
{
    //[SerializeField] private GameObject gun;
    [SerializeField] private GameObject weaponIcon;
    [SerializeField] private GameObject spawnerManager;
    [SerializeField] private GameObject spawners;

    //[SerializeField] private GameObject AbilityHolder;
    [SerializeField] private GameObject abilityIcon;

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject missionManager;

    [SerializeField] private GameObject GlitchDeathCinematicContainer;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private GameObject[] invulnerableEnemies;
    //[SerializeField] private int normalEnemiesCount = 10;

    [SerializeField] private GameObject infoFragments;

    private Weapon gunWeapon; // Referencia al componente Weapon del objeto gun

    private void Awake()
    {
        if (weaponManager != null) weaponManager.CanEquipFirstWeapon = false;
        if (weaponManager != null) weaponManager.CanChangeWeapon = false;
        //if (gun != null)
        //{
        //    gunWeapon = gun.GetComponent<Weapon>();
        //}
    }

    private void Start()
    {
       
        //if (AbilityHolder != null) AbilityHolder.SetActive(false);
        if (abilityIcon != null) abilityIcon.SetActive(false);

        if (missionManager != null) missionManager.SetActive(false);

        //if (gun != null) gun.SetActive(false);
        if (weaponIcon != null) weaponIcon.SetActive(false);

        if (spawnerManager != null) spawnerManager.SetActive(false);
        if (spawners != null) spawners.SetActive(false);

        if (GlitchDeathCinematicContainer != null) GlitchDeathCinematicContainer.SetActive(false);

        if (infoFragments != null) infoFragments.SetActive(false);

    }

    public void ActiveGun()
    {
        //if (gun != null) gun.SetActive(true);
        if (weaponManager != null) weaponManager.CanEquipFirstWeapon = true;
        if (weaponManager != null) weaponManager.EquipWeaponInstant(0);
        if (weaponIcon != null) weaponIcon.SetActive(true);

      
        if (gunWeapon != null)
        {
            HUDManager.Instance.UpdateAmmo(gunWeapon.CurrentAmmo, gunWeapon.TotalAmmo);
            HUDManager.Instance.UpdateWeaponIcon(gunWeapon.Stats.weaponIcon);
            HUDManager.Instance.UpdateWeaponName(gunWeapon.Stats.weaponName);
        }
        else
        {
            Debug.LogWarning("[tutorialSceneController] El componente Weapon no está asignado al objeto gun.");
        }
    }

    public void ActiveRifleAndShotgun()
    {
        if (weaponManager != null)
        {
            weaponManager.CanChangeWeapon = true;
        }

        //if (AbilityHolder != null) AbilityHolder.SetActive(true);
        if (abilityIcon != null) abilityIcon.SetActive(true);
    }

    public void SpawnNormalEnemies()
    {
        foreach (var enemy in normalEnemies)
        {
            enemy.SetActive(true);
        }
    }
    public void EnableGlitchScripts()
    {
        foreach (var enemy in normalEnemies)
        {
            Canvas canvas = enemy.GetComponentInChildren<Canvas>(true); 
            canvas.gameObject.SetActive(true);  
            
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            foreach (MonoBehaviour script in enemy.GetComponents<MonoBehaviour>())
            {
                script.enabled = true;
                
            }          
        }
    }

    public void EnableGlitchScriptsInvulnerables()
    {
        foreach (var enemy in invulnerableEnemies)
        {
            Canvas canvas = enemy.GetComponentInChildren<Canvas>(true);
            canvas.gameObject.SetActive(true);

            enemy.GetComponent<NavMeshAgent>().enabled = true;
            foreach (MonoBehaviour script in enemy.GetComponents<MonoBehaviour>())
            {
                script.enabled = true;

            }
        }
    }

    public void PlayGlitchDeathCinematic()
    {
        //Time.timeScale = 0;
        GlitchDeathCinematicContainer.SetActive(true);
    }

    public void StopGlitchDeathCinematic()
    {           
        GlitchDeathCinematicContainer.SetActive(false);
        //Time.timeScale = 1f;
         // Inicia el siguiente escenario (escena 5)        
    }

    public void ActivateInfoFragments()
    {
        if (infoFragments != null)
        {
            infoFragments.SetActive(true);
        }
    }

    public void ActivateFirstMission()
    {
        Debug.Log("[tutorialSceneController] Activando primera misión.");

        spawners.SetActive(true);
        spawnerManager.SetActive(true);
        missionManager.SetActive(true);
    }

}