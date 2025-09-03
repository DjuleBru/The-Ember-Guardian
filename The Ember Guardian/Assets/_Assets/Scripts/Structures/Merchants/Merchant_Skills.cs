using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Merchant_Skills : Merchant
{

    [SerializeField] private Transform refundDropPosition;
    private List<SkillSO> merchantSkillSOList;
    private List<SkillItem> majorSkillList;
    private List<SkillItem> minorSkillList; // Référence aux skills du joueur

    protected List<SkillItem> majorSkillItemsForSale;
    protected List<SkillItem> minorSkillItemsForSale = new List<SkillItem>();

    protected int drawWeightForBoughtSkills = 3;

    public static event EventHandler OnPlayerRefundedItem;

    protected override void Start() {
        base.Start();
        PlayerSkills.Instance.OnInitialSkillsInitialized += PlayerSkills_OnInitialSkillsInitialized;
    }

    private void PlayerSkills_OnInitialSkillsInitialized(object sender, System.EventArgs e) {
        InitializeMerchantItems();
    }

    protected void InitializeSkillItems() {
        majorSkillList = new List<SkillItem>();
        minorSkillList = new List<SkillItem>();
        allItemsForSale = new List<MerchantItem>();

        merchantSkillSOList = PlayerSave.Instance.GetAllSkillsUnlocked();
        foreach (SkillSO skillSO in merchantSkillSOList) {
            var skillItem = new SkillItem();
            skillItem.Initialize(skillSO);

            // Check if player already has skill (initial skills)
            SkillItem activeSkillItem = PlayerSkills.Instance.GetActiveSkillLeft();
            List<SkillItem> passiveSkillList = PlayerSkills.Instance.GetPassiveSkillList();

            if (activeSkillItem != null && activeSkillItem.skillType == skillSO.skillType) {
                skillItem.currentLevel = PlayerSkills.Instance.GetActiveSkillLeft().currentLevel + 1;
            }
            if (passiveSkillList.Count != 0 && passiveSkillList[0].skillType == skillSO.skillType) {
                skillItem.currentLevel = PlayerSkills.Instance.GetPassiveSkillList()[0].currentLevel + 1;
            }

            if (skillSO.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                allMajorMerchantItems.Add(skillItem);
                majorSkillList.Add(skillItem);
            }

            if (skillSO.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                allMinorMerchantItems.Add(skillItem);
                minorSkillList.Add(skillItem);
            }
        }
    }

    protected override void InitializeMerchantItems() {

        if(useDebugItemAmountToDisplay) {
            bigItemsToDisplayAmount = debugBigItemToDisplay;
            smallItemsToDisplayAmount = debugSmallItemToDisplay;
        } else {
            bigItemsToDisplayAmount = StructureStats.Instance.GetSkillMerchantMaxActiveSkillsDisplayed();
            smallItemsToDisplayAmount = StructureStats.Instance.GetSkillMerchantMaxPassiveSkillsDisplayed();
        }

        InitializeSkillItems();
        RefreshShopItems();
    }

    protected void RefreshSoldSkill(SkillItem skillItem) {
        foreach(SkillItem majorSkillItem in majorSkillList) {
            if (skillItem.itemName == majorSkillItem.itemName) {
                skillItem.currentLevel++;
                skillItem.price = CalculateCost(skillItem);
            }
        }

        foreach (SkillItem minorSkillItem in minorSkillList) {
            if (skillItem.itemName == minorSkillItem.itemName) {
                skillItem.currentLevel++;
                skillItem.price = CalculateCost(skillItem);
            }
        }
    }

    private void SetAllSkillsUnsold() {
        foreach (SkillItem majorSkillItem in majorSkillList) {
            majorSkillItem.Unpurchase();
        }

        foreach (SkillItem minorSkillItem in minorSkillList) {
            minorSkillItem.Unpurchase();
        }
    }

    public override void SetItemSold(MerchantItem merchantItem) {
        SkillItem skillItem = merchantItem as SkillItem;
        RefreshSoldSkill(skillItem);
    }

    protected override void RefreshShopItems() {
        base.RefreshShopItems();

        //Debug.Log("RefreshShopItems");
        SetAllSkillsUnsold();
        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }

    protected void RefreshCurrentMajorItemForSale() {
        List<SkillItem> majorSkillListToDisplay = new List<SkillItem>();

        int activeSkillCount = PlayerSkills.Instance.GetActiveSkillList().Count;

        // Calcul dynamique du weight en fonction du nombre total de skills
        // minWeight = 2 si peu d’éléments, maxWeight = 4 si beaucoup
        int weight = 2 + (this.majorSkillList.Count - 3) * (4 - 2) / (10 - 3);
        weight = Mathf.Clamp(weight, 2, 4); // limite entre 2 et 4

        foreach (SkillItem skillItem in this.majorSkillList) {
            int currentLevel = PlayerSkills.Instance.GetCurrentSkillLevel(skillItem);

            if (currentLevel > 0 && activeSkillCount > 1) {
                // Skill déjà acquis et plus d’un skill actif : appliquer la pondération
                for (int i = 0; i < weight; i++) {
                    majorSkillListToDisplay.Add(skillItem);
                }
            }
            else {
                // Skill pas encore acquis ou unique skill actif : pondération 1
                majorSkillListToDisplay.Add(skillItem);
            }
        }

        majorSkillItemsForSale = DrawSkillsWithoutReplacement(majorSkillListToDisplay, bigItemsToDisplayAmount);
        majorItemListForSale = ConvertSkillListInMerchantItemList(majorSkillItemsForSale);

        foreach (SkillItem skillItem in majorItemListForSale) {
            allItemsForSale.Add(skillItem);
        }
    }

    protected void RefreshCurrentMinorItemListForSale() {
        // Pondération pour augmenter la chance des améliorations
        List<SkillItem> minorSkillList = new List<SkillItem>();

        foreach (SkillItem skillItem in this.minorSkillList) {
            minorSkillList.Add(skillItem);
        }

        // Effectuer le tirage à partir de la liste pondérée
        minorSkillItemsForSale = DrawSkillsWithoutReplacement(minorSkillList, smallItemsToDisplayAmount);
        minorItemListForSale = ConvertSkillListInMerchantItemList(minorSkillItemsForSale);

        foreach (SkillItem skillItem in minorItemListForSale) {
            allItemsForSale.Add(skillItem);
        }
    }

    public List<SkillItem> DrawSkillsWithoutReplacement(List<SkillItem> skillList, int numberOfDraws) {
        // Créer une copie pour ne pas modifier l'original
        List<SkillItem> skillListCopy = new List<SkillItem>(skillList);

        // Vérifie si le nombre de tirages demandé est supérieur à la taille de la liste
        if (numberOfDraws > skillListCopy.Count) {
            Debug.LogWarning("Le nombre de tirages demandé est supérieur à la taille de la liste pondérée.");
            numberOfDraws = skillListCopy.Count;
        }

        // Liste des skills tirés
        List<SkillItem> drawnSkills = new List<SkillItem>();
        HashSet<SkillItem> uniqueDrawnSkills = new HashSet<SkillItem>(); // Pour éviter les doublons

        // Tirage sans remplacement
        while (drawnSkills.Count < numberOfDraws) {
            // Filtrer les éléments déjà tirés
            List<SkillItem> remainingItems = skillListCopy.Where(skillItem => !uniqueDrawnSkills.Contains(skillItem)).ToList();

            if (remainingItems.Count == 0) {
                Debug.LogWarning("Pas assez d'éléments uniques restants pour effectuer le tirage.");
                break;
            }

            // Tirage aléatoire
            int randomIndex = UnityEngine.Random.Range(0, remainingItems.Count);
            SkillItem selectedSkill = remainingItems[randomIndex];

            drawnSkills.Add(selectedSkill);
            uniqueDrawnSkills.Add(selectedSkill);
        }

        return drawnSkills;
    }

    public List<MerchantItem> ConvertSkillListInMerchantItemList(List<SkillItem> list) {
        List<MerchantItem> merchantItemList = new List<MerchantItem>();

        foreach(SkillItem skillItem in list) {
            merchantItemList.Add(skillItem);
        }

        return merchantItemList;
    }

    private int CalculateCost(SkillItem skill) {
        if(skill.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
            return skill.skillSO.passiveSkillEffect.GetPriceAtLevel(skill.currentLevel);
        } else {
            return skill.skillSO.activeSkillEffect.GetPriceAtLevel(skill.currentLevel);
        }
    }
    protected override void TriggerStructurePrimaryFunction() {

        if (!playerPayedToRefreshShop) {
            base.TriggerStructurePrimaryFunction();
            ActivateStructurePrimaryFunctionInteraction(false);

            playerPayedToRefreshShop = true;
            OpenCloseShop(true);

        }
        else {

            InvokeOnPlayerBoughtItem();
        }

        playerJustTriggeredInteraction = true;
    }


    public void RefundPlayer() {
        payCurrencyUI.SetPlayerInteracting(false);
        StartCoroutine(RefundPlayer(payCurrencyUI.GetCurrencyAmountToPay()));

        OnPlayerRefundedItem?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator RefundPlayer(int amountToRefund) {
        for (int i = 0; i < amountToRefund; i++) {

            Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.bigRedOrb);

            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(currencyPrefab, refundDropPosition.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomUpwardsForce(3, 6);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(1f);
            lastBlueOrbDroppedOnTheFloor.SetCanBePickedUpByWorker();

            yield return new WaitForSeconds(.1f);
        }
    }

}
