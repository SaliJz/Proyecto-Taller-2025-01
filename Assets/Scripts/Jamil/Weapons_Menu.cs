using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public Weapons_Cards selectedCard;


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

        public Weapons_Cards GetRandomCard(Weapon_List_Buff_UI wLB)
        {
            // Restaurar el estado de todas las cartas que no est�n seleccionadas
            foreach (Weapons_Cards card in wLB.listInUse)
            {
                if (card.currentState != Weapons_Cards.CurrentState.Selected)
                {
                    card.currentState = Weapons_Cards.CurrentState.InUse;
                }
            }

            int randomIndex = Random.Range(0, wLB.listInUse.Count);
            wLB.listInUse[randomIndex].currentState = Weapons_Cards.CurrentState.Selected;

            return wLB.listInUse[randomIndex];
        }

        public void UpdateCard(TextMeshProUGUI description, TextMeshProUGUI price)
        {
            selectedCard = GetRandomCard(this);
            description.text = selectedCard.buffDescriptionText;
            price.text = selectedCard.price.ToString();
        }

        public bool AreAllBuffListsEmpty()
        {
            return listDamageBuff.Count == 0 &&
                   listRatioFireBuff.Count == 0 &&
                   listReloadSpeedBuff.Count == 0 &&
                   listAmmoBonusBuff.Count == 0;
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

    private void CheckAndDisableUI(Weapon_List_Buff_UI wLB, Button button, TextMeshProUGUI description, TextMeshProUGUI price)
    {
        if (wLB.AreAllBuffListsEmpty())
        {
            button.interactable = false;
            button.image.color = Color.gray;
            description.text = "Nivel m�ximo alcanzado";
            price.text = "-";
        }
        else
        {
            // Hay cartas, verificamos si tiene suficientes fragmentos
            if (HUDManager.Instance.CurrentFragments >= wLB.selectedCard.price)
            {
                button.interactable = true;
                button.image.color = Color.white; // O el color original que uses
            }
            else
            {
                button.interactable = false;
                button.image.color = Color.gray;
            }
        }
    }

    public void BuyGunCard() => BuyCard(gun);
    public void BuyRifleCard() => BuyCard(rifle);
    public void BuyShotgunCard() => BuyCard(shotgun);

    public void BuyCard(Weapon_List_Buff_UI wLB)
    {
            Weapons_Cards selectedCard = wLB.selectedCard; //Usa la misma carta seleccionada en la clase

        if (HUDManager.Instance.CurrentFragments >= selectedCard.price)
        {          
            HUDManager.Instance.DiscountInfoFragment(selectedCard.price);
        }

        //Ahora removemos y agregamos
        //wLB.listInUse.RemoveAll(card => card.currentState == Weapons_Cards.CurrentState.Selected);
        var cardUpgradeType = selectedCard.upgradeType;
        UnityEngine.Debug.Log(cardUpgradeType.ToString());

        if (cardUpgradeType==Weapons_Cards.UpgradeType.WeaponDamage)
        {
            wLB.listDamageBuff.Remove(selectedCard);
            
            int index = wLB.listInUse.IndexOf(selectedCard);
            if (index != -1 && wLB.listDamageBuff.Count > 0)
            {
                wLB.listInUse[index] = wLB.listDamageBuff[0];
            }
        }
        else if (selectedCard.upgradeType == Weapons_Cards.UpgradeType.FireRate)
        {
            wLB.listRatioFireBuff.Remove(selectedCard);
           
            int index = wLB.listInUse.IndexOf(selectedCard);
            if (index != -1 && wLB.listRatioFireBuff.Count > 0)
            {
                wLB.listInUse[index] = wLB.listRatioFireBuff[0];
            }
        }
        else if (selectedCard.upgradeType == Weapons_Cards.UpgradeType.ReloadSpeed)
        {
            wLB.listReloadSpeedBuff.Remove(selectedCard);
            
            int index = wLB.listInUse.IndexOf(selectedCard);
            if (index != -1 && wLB.listReloadSpeedBuff.Count > 0)
            {
                wLB.listInUse[index] = wLB.listReloadSpeedBuff[0];
            }
        }
        else if (selectedCard.upgradeType == Weapons_Cards.UpgradeType.AmmoBonus)
        {
            wLB.listAmmoBonusBuff.Remove(selectedCard);
          
            int index = wLB.listInUse.IndexOf(selectedCard);
            if (index != -1 && wLB.listAmmoBonusBuff.Count > 0)
            {
                wLB.listInUse[index] = wLB.listAmmoBonusBuff[0];
            }
        }

        if (wLB == gun)
        {
            gun.UpdateCard(gunBuffDescriptionTMP, gunPriceTMP);
        }
          
        else if (wLB == rifle)
        {
            rifle.UpdateCard(rifleBuffDescriptionTMP, riflePriceTMP);
        }
          
        else if (wLB == shotgun)
        {
            shotgun.UpdateCard(shotgunBuffDescriptionTMP, shotgunPriceTMP);
        }

        CheckAndDisableUI(gun, gunBuyButton, gunBuffDescriptionTMP, gunPriceTMP);
        CheckAndDisableUI(rifle, rifleBuyButton, rifleBuffDescriptionTMP, riflePriceTMP);
        CheckAndDisableUI(shotgun, shotgunBuyButton, shotgunBuffDescriptionTMP, shotgunPriceTMP);

        /*wLB.listInUse.Add(); *///A�adimmos a la lista de en uso el primero en waiting
                                 //wLB.listWaiting.RemoveAt(0);//Eliminamos ahora el primero de waiting
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene("sceneName");
    }

}
