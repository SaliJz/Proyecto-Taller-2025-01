using UnityEngine;

public class TutorialSceneController : MonoBehaviour
{
    //[SerializeField] private GameObject gun;
    [SerializeField] private GameObject weaponIcon;
    [SerializeField] private GameObject spawnerManager;
    [SerializeField] private GameObject spawners;
    [SerializeField] private GameObject teleporter;
    //[SerializeField] private TutorialManager manager;
    public FadeTransition fadeTransition;
    public HaloMoveController haloMoveController;
    public RotateRig rotateRig;
    public OrbitingCircleSpawner orbitingCircleSpawner;

    [SerializeField] private GameObject abilityIcon;

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject missionManager;

    [SerializeField] private GameObject GlitchDeathCinematicContainer;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private GameObject[] invulnerableEnemies;
    //[SerializeField] private int normalEnemiesCount = 10;

    [SerializeField] private GameObject infoFragments;

    // private Weapon gunWeapon; // Referencia al componente Weapon del objeto gun

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

        //if (spawnerManager != null) spawnerManager.SetActive(false);
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


        // Se ha eliminado el código que actualizaba la munición en el HUD
        // Ya no es necesario
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

    //public void SpawnNormalEnemies()
    //{
    //    StartCoroutine(SpawnWithDelay());
    //}

    //private IEnumerator SpawnWithDelay()
    //{
    //    yield return new WaitForSeconds(0.2f);

    //    if (manager.currentDialogue == 1)
    //    {
    //        foreach (var enemy in normalEnemies)
    //        {
    //            enemy.SetActive(true);
    //            //yield return new WaitForSeconds(0.5f);
    //        }
    //    }

    //    if (manager.currentDialogue == 4)
    //    {
    //        foreach (var enemy in invulnerableEnemies)
    //        {
    //            enemy.SetActive(true);
    //            //yield return new WaitForSeconds(1f);
    //        }
    //    }

    //}

    //public void SpawnInvulnerableEnemies()
    //{
    //    StartCoroutine(SpawnWithDelay());
    //}
    public void EnableGlitchScripts()
    {
        foreach (var enemy in normalEnemies)
        {
            if (enemy == null) continue;

            Canvas canvas = enemy.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }
    }

    public void EnableGlitchScriptsInvulnerables()
    {
        foreach (var enemy in invulnerableEnemies)
        {
            if (enemy == null) continue;

            Canvas canvas = enemy.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
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

        //spawners.SetActive(true);
        //spawnerManager.SetActive(true);
        missionManager.SetActive(true);
    }

    public void DisableTeleport()
    {
        teleporter.SetActive(false);
    }

    public void ActivateFadeOutOnCameraSwitch()
    {
        fadeTransition.fadeDuration = 0.2f;
        fadeTransition.delayBeforeFade = 0;
        fadeTransition.StartCoroutine(fadeTransition.FadeInOut(1));
    }

    public void StartHaloCorutine()
    {
        haloMoveController.StartCoroutine(haloMoveController.MoveHaloAparition());
    }
}