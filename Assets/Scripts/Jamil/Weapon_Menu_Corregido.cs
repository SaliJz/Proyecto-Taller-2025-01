
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Weapon_Menu_Corregido : MonoBehaviour
{
    [SerializeField] private GameObject shopMenu;

    [Header("All Cards")]
    [SerializeField] private List<Weapon_Card_Corregido> gunAllCards;
    [SerializeField] private List<Weapon_Card_Corregido> rifleAllCards;
    [SerializeField] private List<Weapon_Card_Corregido> shotgunAllCards;

    [Header("All Buttons")]
    [SerializeField] private Button buttonGunPrice_UI;
    [SerializeField] private Button buttonRiflePrice_UI;
    [SerializeField] private Button buttonShotgunPrice_UI;
    [SerializeField] private Button buttonExit_UI;

    [Header("TMP Gun")]
    [SerializeField] private TextMeshProUGUI gunDescription_UI_TMP;
    [SerializeField] private TextMeshProUGUI gunPrice_UI_TMP;

    [Header("TMP Rifle")]
    [SerializeField] private TextMeshProUGUI rifleDescription_UI_TMP;
    [SerializeField] private TextMeshProUGUI riflePrice_UI_TMP;

    [Header("TMP Shotgun")]
    [SerializeField] private TextMeshProUGUI shotgunDescription_UI_TMP;
    [SerializeField] private TextMeshProUGUI shotgunPrice_UI_TMP;

    
    [System.Serializable]
    public class Weapon_Manager_Card
    {
        [SerializeField] public List<Weapon_Card_Corregido> weaponCardList;
        [SerializeField] public TextMeshProUGUI descriptionTMP;
        [SerializeField] public TextMeshProUGUI priceTMP;
        [SerializeField] public Button buttonPrice;
        [SerializeField] public int currentIndex=0;
    }

    private Weapon_Manager_Card gunManager=new Weapon_Manager_Card();
    private Weapon_Manager_Card rifleManager=new Weapon_Manager_Card();
    private Weapon_Manager_Card shotgunManager=new Weapon_Manager_Card();

    private void Start()
    {
        MoveInspectorListsToWeaponManagerCard();  
    }

    private void Awake()
    {
        buttonExit_UI.onClick.AddListener(ExitWeaponMenu);
    }

    void MoveInspectorListsToWeaponManagerCard()
    {
        gunManager.weaponCardList = gunAllCards;
        rifleManager.weaponCardList = rifleAllCards;
        shotgunManager.weaponCardList = shotgunAllCards;

        gunManager.descriptionTMP = gunDescription_UI_TMP;
        gunManager.priceTMP = gunPrice_UI_TMP;

        rifleManager.descriptionTMP = rifleDescription_UI_TMP;
        rifleManager.priceTMP = riflePrice_UI_TMP;

        shotgunManager.descriptionTMP = shotgunDescription_UI_TMP;
        shotgunManager.priceTMP = shotgunPrice_UI_TMP;

        gunManager.buttonPrice=buttonGunPrice_UI;
        rifleManager.buttonPrice = buttonRiflePrice_UI;
        shotgunManager.buttonPrice= buttonShotgunPrice_UI;

        Display_Initial_Card(gunManager);
        Display_Initial_Card(rifleManager);
        Display_Initial_Card(shotgunManager);
    }
    void IncreaseIndex(Weapon_Manager_Card currentManagerCard)
    {
        currentManagerCard.currentIndex +=1;
    }
    public void Display_Initial_Card(Weapon_Manager_Card manager)
    {
        Weapon_Card_Corregido currentCard = manager.weaponCardList[0];
        manager.descriptionTMP.text = currentCard.buffDescriptionText;
        manager.priceTMP.text = currentCard.price.ToString();
    }
    void Update_Cards_UI(Weapon_Manager_Card manager)
    {
       
        if(manager.currentIndex>=manager.weaponCardList.Count) return;
        int index= manager.currentIndex;
        Weapon_Card_Corregido currentCard = manager.weaponCardList[index];

        if (HUDManager.Instance.CurrentFragments >= currentCard.price)
        {
            BuyCard(currentCard); //Aqui compramos la carta seleccionada  
            CheckPurchasableCards();
            IncreaseIndex(manager); //Actualizamos el indice para mostrar la siguiente carta

            if (manager.currentIndex >= manager.weaponCardList.Count)
            {
               
                DisableBuyButton(manager.buttonPrice, manager.descriptionTMP,manager.priceTMP, true);
                return;//Si la siguiente carta no esta en la lista devolvemos nada
            }

            else
            {
                index = manager.currentIndex;
                currentCard = manager.weaponCardList[index];
                manager.descriptionTMP.text = currentCard.buffDescriptionText;
                manager.priceTMP.text = currentCard.price.ToString();
            }
        }

        else
        {
            DisableBuyButton(manager.buttonPrice, manager.descriptionTMP,manager.priceTMP, false);
        }
        
            
    }
    public void Update_Gun_Card_UI() => Update_Cards_UI(gunManager);
    public void Update_Rifle_Card_UI() => Update_Cards_UI(rifleManager);
    public void Update_Shotgun_Card_UI() => Update_Cards_UI(shotgunManager);
  
    public void BuyCard(Weapon_Card_Corregido cardToBuy)
    {       
       HUDManager.Instance.DiscountInfoFragment(cardToBuy.price);
       UpgradeDataStore.Instance.ApplyWeaponUpgrade(cardToBuy);
    }
    public void CheckPurchasableCards()
    {
        CheckAndDisable(gunManager);
        CheckAndDisable(rifleManager);
        CheckAndDisable(shotgunManager);
    }
    private void CheckAndDisable(Weapon_Manager_Card manager)
    {
        if (manager.weaponCardList == null) return;

        if (manager.currentIndex < manager.weaponCardList.Count)
        {
            var card = manager.weaponCardList[manager.currentIndex];
            if (card.price >= HUDManager.Instance.CurrentFragments)
            {
                DisableBuyButton(manager.buttonPrice, manager.descriptionTMP,manager.priceTMP, false);
            }
        }
    }
    public void DisableBuyButton(Button buttonToDeactivate, TextMeshProUGUI description, TextMeshProUGUI price, bool reachedMaximunLevel)
    {
        buttonToDeactivate.interactable = false;
        buttonToDeactivate.image.color = Color.gray;

        if (reachedMaximunLevel)
        {
           description.text = "Nivel máximo alcanzado";
           price.text = "";
        }
      
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitWeaponMenu()
    {
        gameObject.SetActive(false);
        shopMenu.SetActive(true);
    }
}
