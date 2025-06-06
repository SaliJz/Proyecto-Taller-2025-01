using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Weapons_Menu : MonoBehaviour
{
    [Header("Cartas totales (ordenadas manualmente por ID)")]
    [SerializeField] private List<Weapons_Cards> gunAllCards;
    [SerializeField] private List<Weapons_Cards> rifleAllCards;
    [SerializeField] private List<Weapons_Cards> shotgunAllCards;

    [Header("UI Gun")]
    [SerializeField] private TextMeshProUGUI gunBuffDescriptionTMP;
    [SerializeField] private TextMeshProUGUI gunPriceTMP;
    [SerializeField] private Button gunBuyButton;

    [Header("UI Rifle")]
    [SerializeField] private TextMeshProUGUI rifleBuffDescriptionTMP;
    [SerializeField] private TextMeshProUGUI riflePriceTMP;
    [SerializeField] private Button rifleBuyButton;

    [Header("UI Shotgun")]
    [SerializeField] private TextMeshProUGUI shotgunBuffDescriptionTMP;
    [SerializeField] private TextMeshProUGUI shotgunPriceTMP;
    [SerializeField] private Button shotgunBuyButton;

    [Header("Fragmentos del jugador")]
    [SerializeField] private int playerFragments = 100;

    [System.Serializable]
    public class Weapon_List_Buff_UI
    {
        public List<Weapons_Cards> listDamageBuff = new List<Weapons_Cards>();
        public List<Weapons_Cards> listtRatioFireBuff = new List<Weapons_Cards>();
        public List<Weapons_Cards> listReloadSpeedBuff = new List<Weapons_Cards>();
        public List<Weapons_Cards> listAmmoBonusBuff = new List<Weapons_Cards>();

        public List<Weapons_Cards> listInUse = new List<Weapons_Cards>();
        public List<Weapons_Cards> listWaiting = new List<Weapons_Cards>();
        public List<Weapons_Cards> listUsed = new List<Weapons_Cards>();


        public void GenerateBuffList(List<Weapons_Cards> weaponList)
        {
            listDamageBuff.Clear();
            listtRatioFireBuff.Clear();
            listReloadSpeedBuff.Clear();
            listAmmoBonusBuff.Clear();

            foreach (Weapons_Cards newCard in weaponList)
            {
                if (newCard.upgradeType == Weapons_Cards.UpgradeType.WeaponDamage)
                    listDamageBuff.Add(newCard);
                else if (newCard.upgradeType == Weapons_Cards.UpgradeType.FireRate)
                    listtRatioFireBuff.Add(newCard);
                else if (newCard.upgradeType == Weapons_Cards.UpgradeType.ReloadSpeed)
                    listReloadSpeedBuff.Add(newCard);
                else if (newCard.upgradeType == Weapons_Cards.UpgradeType.AmmoBonus)
                    listAmmoBonusBuff.Add(newCard);
            }
        }

        //Agregamos elementos a la lista listInUse
        public void GroupIndexes0(List<Weapons_Cards> weaponList) 
        {
            foreach (Weapons_Cards newCard in weaponList)
            {
                if (newCard.ID == 0)
                {
                    newCard.currentState = Weapons_Cards.CurrentState.InUse;
                    listInUse.Add(newCard);               
                }
            }

            //weaponList.RemoveAll(elemento => elemento.ID == 0);
        }

        //Agregamos elementos a la lista listWaiting
        public void GroupWaitingState(List<Weapons_Cards> weaponList)
        {
            foreach (Weapons_Cards newCard in weaponList)
            {
                if (newCard.currentState == Weapons_Cards.CurrentState.Waiting)
                {
                    listWaiting.Add(newCard);
                }
            }
        }
    }

    public Weapon_List_Buff_UI gun = new Weapon_List_Buff_UI();
    public Weapon_List_Buff_UI rifle = new Weapon_List_Buff_UI();
    public Weapon_List_Buff_UI shotgun = new Weapon_List_Buff_UI();


    private void Start()
    {
        playerFragments = HUDManager.Instance.CurrentFragments;

        gun.GenerateBuffList(gunAllCards);
        rifle.GenerateBuffList(rifleAllCards);
        shotgun.GenerateBuffList(shotgunAllCards);


        gun.GroupIndexes0(gunAllCards);
        rifle.GroupIndexes0(rifleAllCards);
        shotgun.GroupIndexes0(shotgunAllCards);

        gun.GroupWaitingState(gunAllCards);
        rifle.GroupWaitingState(rifleAllCards);
        shotgun.GroupWaitingState(shotgunAllCards);


    }

    public void ShowCurrentCard()
    {
        //UpdateCard(gunAllCards);
        //UpdateCard(rifleAllCards);
        //UpdateCard(shotgunAllCards);
    }
  

    public void ManageCards(Weapon_List_Buff_UI WLB)
    {
       
        //if(WLB.)
    }
    //private void UpdateCard(List<Weapons_Cards> weaponList)
    //{

        
    //    var card = weaponList[0];




    //    if (weaponList.Count > 0)
    //    {
    //        var card = weaponList[0];
    //        description.text = card.buffDescriptionText;
    //        price.text = card.price.ToString();
    //        button.interactable = playerFragments >= card.price;
    //    }
    //    else
    //    {
    //        description.text = "Sin cartas disponibles";
    //        price.text = "";
    //        price.color = Color.gray;
    //        button.interactable = false;
    //    }
    //}

    //public void BuyGunCard() => BuyCard(gunAllCards, gunWaiting, gunUsed, gunBuffDescriptionTMP, gunPriceTMP, gunBuyButton);
    //public void BuyRifleCard() => BuyCard(rifleAllCards, rifleWaiting, rifleUsed, rifleBuffDescriptionTMP, riflePriceTMP, rifleBuyButton);
    //public void BuyShotgunCard() => BuyCard(shotgunAllCards, shotgunWaiting, shotgunUsed, shotgunBuffDescriptionTMP, shotgunPriceTMP, shotgunBuyButton);

    //private void BuyCard(List<Weapons_Cards> allCards, List<Weapons_Cards> waiting, List<Weapons_Cards> used,
    //                     TextMeshProUGUI desc, TextMeshProUGUI price, Button button)
    //{
    //    if (waiting.Count == 0) return;

    //    var current = waiting[0];
    //    if (playerFragments >= current.price)
    //    {
    //        playerFragments -= current.price;
    //        HUDManager.Instance.AddInfoFragment(-current.price);
    //        used.Add(current);
    //        waiting.RemoveAt(0);

    //        // Reemplazo: buscar una carta del mismo tipo que no esté en uso ni usada
    //        foreach (var card in allCards)
    //        {
    //            if (card.upgradeType == current.upgradeType && !waiting.Contains(card) && !used.Contains(card))
    //            {
    //                waiting.Add(card);
    //                break;
    //            }
    //        }

    //        ShowCard(waiting, desc, price, button);
    //    }
    //}
}
