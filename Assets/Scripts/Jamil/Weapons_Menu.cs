using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapons_Menu : MonoBehaviour
{
    [Header("Listas de cartas")]
    [SerializeField] private List<Weapons_Cards> gunCardsList;
    [SerializeField] private List<Weapons_Cards> rifleCardsList;
    [SerializeField] private List<Weapons_Cards> shotgunCardsList;

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

    [Header("Índices actuales")]
    [SerializeField] private int gunCurrentIndex = 0;
    [SerializeField] private int rifleCurrentIndex = 0;
    [SerializeField] private int shotgunCurrentIndex = 0;

    private void Start()
    {
        playerFragments = HUDManager.Instance.CurrentFragments;
        ShowCurrentCardDescription();
    }

    public void ShowCurrentCardDescription()
    {
        UpdateCard(gunCardsList, gunCurrentIndex, gunBuffDescriptionTMP, gunPriceTMP, gunBuyButton);
        UpdateCard(rifleCardsList, rifleCurrentIndex, rifleBuffDescriptionTMP, riflePriceTMP, rifleBuyButton);
        UpdateCard(shotgunCardsList, shotgunCurrentIndex, shotgunBuffDescriptionTMP, shotgunPriceTMP, shotgunBuyButton);
    }

    private void UpdateCard(List<Weapons_Cards> list, int index, TextMeshProUGUI descriptionTMP, TextMeshProUGUI priceTMP, Button buyButton)
    {
        if (list != null && index < list.Count)
        {
            var card = list[index];
            descriptionTMP.text = card.buffDescriptionText;
            priceTMP.text = card.price.ToString();
            buyButton.interactable = playerFragments >= card.price;
            //priceTMP.color = Color.white;
        }
        else
        {
            // No hay más cartas disponibles
            descriptionTMP.text = "Sin cartas disponibles";
            priceTMP.text = "";
            priceTMP.color = Color.gray;
            buyButton.interactable = false;
        }
    }

    public void BuyGunCard() => TryBuy(gunCardsList, ref gunCurrentIndex, gunBuffDescriptionTMP, gunPriceTMP, gunBuyButton);
    public void BuyRifleCard() => TryBuy(rifleCardsList, ref rifleCurrentIndex, rifleBuffDescriptionTMP, riflePriceTMP, rifleBuyButton);
    public void BuyShotgunCard() => TryBuy(shotgunCardsList, ref shotgunCurrentIndex, shotgunBuffDescriptionTMP, shotgunPriceTMP, shotgunBuyButton);

    private void TryBuy(List<Weapons_Cards> list, ref int index, TextMeshProUGUI descriptionTMP, TextMeshProUGUI priceTMP, Button buyButton)
    {
        if (list != null && index < list.Count)
        {
            var card = list[index];
            if (playerFragments >= card.price)
            {
                playerFragments -= card.price;
                HUDManager.Instance.AddInfoFragment(-card.price);

                Debug.Log("Compra realizada: " + card.name + " | Fragmentos restantes: " + playerFragments);
                index++;

                // Mostrar la siguiente carta o desactivar
                if (index < list.Count)
                {
                    UpdateCard(list, index, descriptionTMP, priceTMP, buyButton);
                   
                }
                else
                {
                    descriptionTMP.text = "Sin cartas disponibles";
                    priceTMP.text = "";
                    priceTMP.color = Color.gray;
                    buyButton.interactable = false;
                }
            }
        }
    }
}
