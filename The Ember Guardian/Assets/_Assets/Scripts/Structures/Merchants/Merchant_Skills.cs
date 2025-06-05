using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Merchant_Skills : Merchant
{

    private List<SkillSO> merchantSkillSOList;
    private List<SkillItem> majorSkillList;
    private List<SkillItem> minorSkillList; // Référence aux skills du joueur

    protected List<SkillItem> majorSkillItemsForSale;
    protected List<SkillItem> minorSkillItemsForSale = new List<SkillItem>();

    protected int drawWeightForBoughtSkills = 3;

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
        //Debug.Log("RefreshShopItems");
        SetAllSkillsUnsold();
        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }

    protected void RefreshCurrentMajorItemForSale() {
        //Debug.Log("RefreshCurrentMajorItemForSale");
        // Pondération pour augmenter la chance des améliorations
        List<SkillItem> majorSkillListToDisplay = new List<SkillItem>();

        // Le joueur a moins de 2 skills actifs : on propose encore de nouveaux skills
        if (PlayerSkills.Instance.GetActiveSkillList().Count < 2) {
            foreach (SkillItem skillItem in this.majorSkillList) {
                majorSkillListToDisplay.Add(skillItem);
            }
        } else {
            // Le joueur a 2 skills actifs : on propose uniquement des améliorations

            foreach (SkillItem skillItem in this.majorSkillList) {
                if (PlayerSkills.Instance.GetCurrentSkillLevel(skillItem) > 0) {
                    majorSkillListToDisplay.Add(skillItem);
                }
            }
        }

        majorSkillItemsForSale = DrawSkillsWithoutReplacement(majorSkillListToDisplay, bigItemsToDisplayAmount);
        majorItemListForSale = ConvertSkillListInMerchantItemList(majorSkillItemsForSale);

        foreach(SkillItem skillItem in majorItemListForSale) {
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
        // Créer une copie de la liste pour ne pas modifier l'original
        List<SkillItem> skillListCopy = new List<SkillItem>(skillList);

        // Créer une liste de pondérations basée sur les éléments
        List<SkillItem> weightedList = new List<SkillItem>();

        // Remplir la liste pondérée sans duplication des éléments, en fonction des poids
        foreach (SkillItem skillItem in skillListCopy) {
            int weight = 4; // Pondération pour les améliorations
            for (int i = 0; i < weight; i++) {
                weightedList.Add(skillItem);
            }
        }

        // Vérifie si le nombre de tirages demandé est supérieur à la taille de la liste
        if (numberOfDraws > weightedList.Count) {
            Debug.LogWarning("Le nombre de tirages demandé est supérieur à la taille de la liste pondérée.");
            numberOfDraws = weightedList.Count; // Limite le nombre de tirages
        }

        // Liste des skills tirés
        List<SkillItem> drawnSkills = new List<SkillItem>();
        HashSet<SkillItem> uniqueDrawnSkills = new HashSet<SkillItem>(); // Pour vérifier les doublons

        // Effectuer le tirage sans remplacement
        while (drawnSkills.Count < numberOfDraws) {
            // Filtrer la liste pour ne garder que les éléments qui n'ont pas encore été tirés
            List<SkillItem> remainingItems = weightedList.Where(skillItem => !uniqueDrawnSkills.Contains(skillItem)).ToList();

            // Si il n'y a pas assez d'éléments uniques restants pour compléter le tirage, on arrête
            if (remainingItems.Count == 0) {
                Debug.LogWarning("Pas assez d'éléments uniques restants pour effectuer le tirage.");
                break;
            }

            // Tirage d'un élément aléatoire parmi les éléments restants
            int randomIndex = Random.Range(0, remainingItems.Count);
            SkillItem selectedSkill = remainingItems[randomIndex];

            // Ajouter le skill à la liste et le marquer comme tiré
            drawnSkills.Add(selectedSkill);
            uniqueDrawnSkills.Add(selectedSkill); // Marquer comme tiré
        }

        return drawnSkills; // Retourne la liste des skills tirés
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
}
