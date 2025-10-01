using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant_Traps : Merchant
{
    [SerializeField] private Transform trapCollectibleSpawnPosition;

    private List<TrapItem> trapList;
    private List<TrapItem> trapUpgradeList;

    protected List<TrapItem> trapListForSale = new List<TrapItem>();
    protected List<TrapItem> trapUpgradeListForSale = new List<TrapItem>();

    protected override void Start() {
        base.Start();
        MerchantItem.OnAnyMerchantItemBought += MerchantItem_OnAnyMerchantItemBought;
        TrapManager.Instance.OnTrapUpgradeLevelsSet += TrapManager_OnTrapUpgradeLevelsSet;
    }


    private void MerchantItem_OnAnyMerchantItemBought(object sender, System.EventArgs e) {
        if (!(sender is TrapItem)) return;
        TrapItem trapItem = (TrapItem)sender;

        if(trapItem.itemType == MerchantItem.MerchantItemType.Trap) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(trapItem.trapType), trapCollectibleSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(.5f);
            collectible.ApplyRandomSidewardsForce(3, 15);
        }
    }

    private void TrapManager_OnTrapUpgradeLevelsSet(object sender, EventArgs e) {
        InitializeTrapItems();
        RefreshShopItems();
    }

    protected void InitializeTrapItems() {
        trapList = new List<TrapItem>();
        trapUpgradeList = new List<TrapItem>();
        trapListForSale = new List<TrapItem>();

        foreach (TrapSO trapSO in TrapManager.Instance.GetUnlockedTrapsAndTheirUpgrades()) {
            var trapItem = new TrapItem();
            trapItem.Initialize(trapSO);

            if (trapSO.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
                // Synchroniser le currentLevel avec le TrapManager
                int savedLevel = TrapManager.Instance.GetTrapUpgradesLevels()[trapSO.trapType][trapSO.trapUpgradeSO.trapUpgradeType];

                trapItem.currentLevel = savedLevel+1;
                trapItem.price = trapSO.trapUpgradeSO.GetPriceAtLevel(savedLevel);

                allMinorMerchantItems.Add(trapItem);
                trapUpgradeList.Add(trapItem);
            }

            if (trapSO.itemType == MerchantItem.MerchantItemType.Trap) {
                allMajorMerchantItems.Add(trapItem);
                trapList.Add(trapItem);
            }
        }
    }

    protected override void InitializeMerchantItems() {
        if (useDebugItemAmountToDisplay) {
            bigItemsToDisplayAmount = debugBigItemToDisplay;
            smallItemsToDisplayAmount = debugSmallItemToDisplay;
        }
        else {
            bigItemsToDisplayAmount = StructureStats.Instance.GetTrapMerchantMaxTrapsDisplayed();
            smallItemsToDisplayAmount = StructureStats.Instance.GetTrapMerchantMaxTrapUpgradesDisplayed();
        }

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

    private void SetAllTrapsUnsold() {
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
        base.RefreshShopItems();

        SetAllTrapsUnsold();
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
        // Récupérer uniquement les traps upgrades pour des traps déjà achetées par le joueur ou pour la trap proposée
        List<TrapItem> trapUpgradesToPropose = GetEligibleTrapUpgradeItems();

        // Effectuer le tirage à partir de la liste pondérée
        trapUpgradeListForSale = DrawTrapsWithoutReplacement(trapUpgradesToPropose, smallItemsToDisplayAmount);
        minorItemListForSale = ConvertTrapListInMerchantItemList(trapUpgradeListForSale);

        foreach (TrapItem trapItem in minorItemListForSale) {
            allItemsForSale.Add(trapItem);
        }
    }

    public List<TrapItem> GetEligibleTrapUpgradeItems() {
        List<TrapItem> trapListCopy = new List<TrapItem>();
        List<TrapItem> eligibleTrapUpgradeItems = new List<TrapItem>();

        foreach (TrapItem trapItem in trapUpgradeList) {
            trapListCopy.Add(trapItem);
        }

        // Liste des TrapTypes déjà achetés ou proposés en "major items"
        List<TrapItem.TrapType> eligibleTrapTypes = new List<TrapItem.TrapType>();

        // Ajouter les pièges en vente dans les major items
        foreach (TrapItem majorItem in majorItemListForSale) {
            if (!eligibleTrapTypes.Contains(majorItem.trapType)) {
                eligibleTrapTypes.Add(majorItem.trapType);
            }
        }

        // Ajouter les pièges déjà achetés par le joueur
        foreach (TrapItem.TrapType trapType in Enum.GetValues(typeof(TrapItem.TrapType))) {
            if (TrapManager.Instance.TrapTypeBoughtByPlayer(trapType) && !eligibleTrapTypes.Contains(trapType)) {
                eligibleTrapTypes.Add(trapType);
            }
        }

        // Récupérer toutes les upgrades correspondant aux pièges éligibles
        foreach (TrapItem trapItem in trapListCopy) {
            if(eligibleTrapTypes.Contains(trapItem.trapType)) {
                eligibleTrapUpgradeItems.Add(trapItem);
            }
        }


        return eligibleTrapUpgradeItems;
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
            int randomIndex = UnityEngine.Random.Range(0, trapListCopy.Count); // Sélectionne un index aléatoire
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
