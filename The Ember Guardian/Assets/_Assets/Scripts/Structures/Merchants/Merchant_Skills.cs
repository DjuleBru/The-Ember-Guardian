using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant_Skills : Merchant
{
    [SerializeField] private List<SkillSO> merchantSkillSOList;
    private List<SkillItem> majorSkillList;
    private List<SkillItem> minorSkillList; // Référence aux skills du joueur

    protected List<SkillItem> majorSkillItemsForSale;
    protected List<SkillItem> minorSkillItemsForSale = new List<SkillItem>();

    protected void InitializeSkillItems() {
        majorSkillList = new List<SkillItem>();
        minorSkillList = new List<SkillItem>();
        allItemsForSale = new List<MerchantItem>();

        foreach (SkillSO skillSO in merchantSkillSOList) {
            var skillItem = new SkillItem();
            skillItem.Initialize(skillSO);

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

        foreach (SkillItem majorSkillItem in minorSkillList) {
            if (skillItem.itemName == majorSkillItem.itemName) {
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
        SetAllSkillsUnsold();
        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }

    protected void RefreshCurrentMajorItemForSale() {
        majorSkillItemsForSale = DrawSkillsWithoutReplacement(majorSkillList,1);
        majorItemListForSale = ConvertSkillListInMerchantItemList(majorSkillItemsForSale);

        foreach(SkillItem skillItem in majorItemListForSale) {
            allItemsForSale.Add(skillItem);
        }
    }

    protected void RefreshCurrentMinorItemListForSale() {
        minorSkillItemsForSale = DrawSkillsWithoutReplacement(minorSkillList, 3);
        minorItemListForSale = ConvertSkillListInMerchantItemList(minorSkillItemsForSale);

        foreach (SkillItem skillItem in minorItemListForSale) {
            allItemsForSale.Add(skillItem);
        }
    }

    public List<SkillItem> DrawSkillsWithoutReplacement(List<SkillItem> skillList, int numberOfDraws) {
        List<SkillItem> skillListCopy = new List<SkillItem>();
        foreach(SkillItem skillItem in skillList) {
            skillListCopy.Add(skillItem);
        }

        // Vérifie si le nombre de tirages demandé est supérieur à la taille de la liste
        if (numberOfDraws > skillListCopy.Count) {
            Debug.LogWarning("Le nombre de tirages demandé est supérieur à la taille de la liste.");
            numberOfDraws = skillListCopy.Count; // Limite le nombre de tirages
        }

        // Liste des skills tirés
        List<SkillItem> drawnSkills = new List<SkillItem>();

        // Effectue le tirage
        for (int i = 0; i < numberOfDraws; i++) {
            int randomIndex = Random.Range(0, skillListCopy.Count); // Sélectionne un index aléatoire
            SkillItem selectedSkill = skillListCopy[randomIndex]; // Récupère le skill correspondant
            drawnSkills.Add(selectedSkill); // Ajoute le skill à la liste des tirages
            skillListCopy.RemoveAt(randomIndex); // Supprime le skill tiré de la liste originale
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
        return skill.price + skill.currentLevel-1; // Exemple simple : coût basé sur le niveau
    }
}
