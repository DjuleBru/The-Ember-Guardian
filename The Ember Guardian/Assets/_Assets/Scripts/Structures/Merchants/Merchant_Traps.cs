using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant_Traps : Merchant
{
    [SerializeField] private List<TrapSO> trapSOList;
    [SerializeField] private Transform trapCollectibleSpawnPosition;

    private List<TrapItem> trapList;
    private List<TrapItem> trapUpgradeList;

    protected List<TrapItem> trapListForSale = new List<TrapItem>();
    protected List<TrapItem> trapUpgradeListForSale = new List<TrapItem>();

    protected override void Start() {
        base.Start();
        MerchantItem.OnAnyMerchantItemBought += MerchantItem_OnAnyMerchantItemBought;
    }

    private void MerchantItem_OnAnyMerchantItemBought(object sender, System.EventArgs e) {
        if (!(sender is TrapItem)) return;
        TrapItem trapItem = (TrapItem)sender;

        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(trapItem.trapType), trapCollectibleSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.SetCollectibleUnInteractable(.5f);
        collectible.ApplyRandomSidewardsForce(3, 15);
    }

    protected void InitializeTrapItems() {
        trapList = new List<TrapItem>();
        trapUpgradeList = new List<TrapItem>();
        trapListForSale = new List<TrapItem>();

        foreach (TrapSO trapSO in trapSOList) {
            var trapItem = new TrapItem();
            trapItem.Initialize(trapSO);

            if (trapSO.itemType == MerchantItem.MerchantItemType.Trap) {
                allMajorMerchantItems.Add(trapItem);
                trapList.Add(trapItem);
            }

            if (trapSO.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
                allMinorMerchantItems.Add(trapItem);
                trapUpgradeList.Add(trapItem);
            }
        }
    }

    protected override void InitializeMerchantItems() {
        InitializeTrapItems();
        RefreshShopItems();
    }

    protected void RefreshSoldSkill(TrapItem trapItem) {
        foreach (TrapItem trapUpgradeItem in trapUpgradeList) {
            if (trapItem.itemName == trapUpgradeItem.itemName) {
                trapItem.currentLevel++;
                trapItem.price = CalculateCost(trapItem);
            }
        }
    }

    private void SetAllSkillsUnsold() {
        foreach (TrapItem trapItem in trapList) {
            trapItem.Unpurchase();
        }

        foreach (TrapItem trapUpgrade in trapUpgradeList) {
            trapUpgrade.Unpurchase();
        }
    }
    public override void SetItemSold(MerchantItem merchantItem) {
        TrapItem trapItem = merchantItem as TrapItem;
        RefreshSoldSkill(trapItem);
    }

    protected override void RefreshShopItems() {
        SetAllSkillsUnsold();
        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }
    protected void RefreshCurrentMajorItemForSale() {
        // Pondération pour augmenter la chance des améliorations

        trapListForSale = DrawTrapsWithoutReplacement(trapList, bigItemsToDisplayAmount);
        majorItemListForSale = ConvertTrapListInMerchantItemList(trapListForSale);

        foreach (TrapItem trapItem in majorItemListForSale) {
            allItemsForSale.Add(trapItem);
        }
    }

    protected void RefreshCurrentMinorItemListForSale() {
        // Pondération pour augmenter la chance des améliorations

        // Effectuer le tirage à partir de la liste pondérée
        trapUpgradeListForSale = DrawTrapsWithoutReplacement(trapUpgradeList, smallItemsToDisplayAmount);
        minorItemListForSale = ConvertTrapListInMerchantItemList(trapUpgradeListForSale);

        foreach (TrapItem trapItem in minorItemListForSale) {
            allItemsForSale.Add(trapItem);
        }
    }

    public List<TrapItem> DrawTrapsWithoutReplacement(List<TrapItem> trapList, int numberOfDraws) {
        List<TrapItem> trapListCopy = new List<TrapItem>();
        foreach (TrapItem trapItem in trapList) {
            trapListCopy.Add(trapItem);
        }

        // Vérifie si le nombre de tirages demandé est supérieur à la taille de la liste
        if (numberOfDraws > trapListCopy.Count) {
            Debug.LogWarning("Le nombre de tirages demandé est supérieur à la taille de la liste.");
            numberOfDraws = trapListCopy.Count; // Limite le nombre de tirages
        }

        // Liste des skills tirés
        List<TrapItem> drawnTraps = new List<TrapItem>();

        // Effectue le tirage
        for (int i = 0; i < numberOfDraws; i++) {
            int randomIndex = Random.Range(0, trapListCopy.Count); // Sélectionne un index aléatoire
            TrapItem selectedTrap = trapListCopy[randomIndex]; // Récupère le skill correspondant
            drawnTraps.Add(selectedTrap); // Ajoute le skill à la liste des tirages
            trapListCopy.RemoveAt(randomIndex); // Supprime le skill tiré de la liste originale
        }

        return drawnTraps; // Retourne la liste des skills tirés
    }

    public List<MerchantItem> ConvertTrapListInMerchantItemList(List<TrapItem> list) {
        List<MerchantItem> merchantItemList = new List<MerchantItem>();

        foreach (TrapItem trapItem in list) {
            merchantItemList.Add(trapItem);
        }

        return merchantItemList;
    }

    private int CalculateCost(TrapItem trapItem) {

        if (trapItem.itemType == TrapItem.MerchantItemType.TrapUpgrade) {
            return trapItem.trapSO.trapUpgradeSO.GetPriceAtLevel(trapItem.currentLevel);
        }
        return trapItem.price;

    }

    private void OnDestroy() {
        MerchantItem.OnAnyMerchantItemBought -= MerchantItem_OnAnyMerchantItemBought;
    }
}
