using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<Weapons_Cards> listRatioFireBuff = new List<Weapons_Cards>();
        public List<Weapons_Cards> listReloadSpeedBuff = new List<Weapons_Cards>();
        public List<Weapons_Cards> listAmmoBonusBuff = new List<Weapons_Cards>();

        public List<Weapons_Cards> listInUse = new List<Weapons_Cards>();
        public List<Weapons_Cards> listWaiting = new List<Weapons_Cards>();
        public List<Weapons_Cards> listUsed = new List<Weapons_Cards>();


        public void GenerateBuffList(List<Weapons_Cards> weaponList)
        {
            listDamageBuff.Clear();
            listRatioFireBuff.Clear();
            listReloadSpeedBuff.Clear();
            listAmmoBonusBuff.Clear();

            foreach (Weapons_Cards newCard in weaponList)
            {
                if (newCard.upgradeType == Weapons_Cards.UpgradeType.WeaponDamage)
                    listDamageBuff.Add(newCard);
                else if (newCard.upgradeType == Weapons_Cards.UpgradeType.FireRate)
                    listRatioFireBuff.Add(newCard);
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

        public Weapons_Cards SelectedCardUI()
        {
            int randomIndex = Random.Range(0,listInUse.Count);
            return listInUse[randomIndex];
        }

        public void UpdateCard(TextMeshProUGUI description, TextMeshProUGUI price)
        {
            var card = SelectedCardUI();
            description.text = card.buffDescriptionText;
            price.text = card.price.ToString();
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

        gun.UpdateCard(gunBuffDescriptionTMP, gunPriceTMP);
        rifle.UpdateCard(rifleBuffDescriptionTMP, riflePriceTMP);
        shotgun.UpdateCard(shotgunBuffDescriptionTMP, shotgunPriceTMP);

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

    /*
    public void BuyGunCard() => BuyCard(gunAllCards);
    public void BuyRifleCard() => BuyCard(rifleAllCards);
    public void BuyShotgunCard() => BuyCard(shotgunAllCards);

    private void BuyCard(Weapons_Cards weaponCard)
    {
        if (playerFragments >= current.price)
        {
            playerFragments -= current.price;
            HUDManager.Instance.AddInfoFragment(-current.price);
            used.Add(current);
            waiting.RemoveAt(0);

            // Reemplazo: buscar una carta del mismo tipo que no esté en uso ni usada
            foreach (var card in weaponList)
            {
                if (card.upgradeType == current.upgradeType && !waiting.Contains(card) && !used.Contains(card))
                {
                    waiting.Add(card);
                    break;
                }
            }

            ShowCard(waiting, desc, price, button);
        }
    }
    */
}
