using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_MushroomMerchantItem : HubMerchantItem
{
    public enum HubMerchantItem_MushroomMerchantItemType {
        passiveDmgIncreaseToCreaturesNotInFireLight,
        passiveShootOnReload,
        passiveLastBulletDealsTwiceDamage,
        passiveMeleeAttackMagmaShot,
        passiveDamageIncreaseInLight,
        passiveAmmoGeneration,
        passiveChanceToDropDoubleXP,
        passiveShieldGenerator,

        activeMagmaShotBullet,
        activeDarkSword,
        activeDarkFlame,
        activeReaper,
        activePlantMine,
        activeHealOnKills,
        activeFeedFireOnKills,
        activeWorkerAttackSpeedBuff,

        bladeTrap,
        bearTrap,
        shockerEjector,
        flameEjector,
        smokeEjector,

        startWith1RandomActiveSkill,
        startWith1RandomPassiveSkill,
        startWith1RandomTrap,

        skillsMerchantMaxActiveSkillsDisplayed,
        skillsMerchantMaxPassiveSkillsDisplayed,
        trapMerchantMaxTrapsDisplayed,
        trapMerchantMaxTrapUpgradesDisplayed,
    }

    public enum HubMerchantItem_MushroomMerchantItemCategory {
        newActiveSkill,
        newPassiveSkill,
        newTrap,
        startWithItem,
        levelMerchantUpgrade,
    }

    [SerializeField] private HubMerchantItem_MushroomMerchantItemType mushroomItemType;
    [SerializeField] private HubMerchantItem_MushroomMerchantItemCategory mushroomItemCategory;
    [SerializeField] private SkillSO linkedSkillSO;
    [SerializeField] private TrapSO linkedTrapSO;

    protected override void Awake() {
        base.Awake();
        RefreshStatValues();
    }

    public override void BuyItem() {
        if (mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.levelMerchantUpgrade || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.startWithItem) {
            SetNewStatIncreaseStats();
        }

        if(mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newActiveSkill || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newPassiveSkill) {
            PlayerSave.Instance.HUBUnlockNewSkill(linkedSkillSO);
        }

        if (mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newTrap) { 
            TrapManager.Instance.HUBUnlockNewTrap(linkedTrapSO);
        }

        base.BuyItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    public override void UpgradeItem() {
        if (mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.levelMerchantUpgrade || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.startWithItem) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void SetNewStatIncreaseStats() {
        float buff = linkedStatModifierSO.statModifierList[itemLevel];

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxActiveSkillsDisplayed) {
            StructureStats.Instance.SetSkillsMerchantMaxActiveSkillsDisplayed((int)buff);
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxPassiveSkillsDisplayed) {
            StructureStats.Instance.SetSkillsMerchantMaxPassiveSkillsDisplayed((int)buff);
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapsDisplayed) {
            StructureStats.Instance.SetTrapsMerchantMaxTrapsDisplayed((int)buff);
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapUpgradesDisplayed) {
            StructureStats.Instance.SetTrapsMerchantMaxTrapUpgradesDisplayed((int)buff);
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomTrap) {
            StructureStats.Instance.SetStartWithRandomTrapAmount((int)buff);
        }
        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomActiveSkill) {
            PlayerStats.Instance.SetInitialRandomActiveSkillLevel((int)buff);
        }
        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomPassiveSkill) {
            PlayerStats.Instance.SetInitialRandomPassiveSkillLevel((int)buff);
        }


    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();
        statModifiedBools.Add(false);
        statValues.Add("");
        statModifiedBools.Add(false);
        statValues.Add("");

        if (mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newActiveSkill || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newPassiveSkill || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newTrap) {
            statModifiedBools.Add(false);
            statValues.Add(""); 
            statModifiedBools.Add(false);
            statValues.Add("");
        }
        if (mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.levelMerchantUpgrade || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.startWithItem) {

            maxItemLevel = linkedStatModifierSO.statModifierList.Count;

            string totalStatValue = "";
            string currentStatValue = "";
            string initialStatPrefix = "";
            string totalStatWithModifierPrefix = "";
            string totalStatWithModifierPostfix = "";
            string relativeStatPostfix = "";
            string relativeStatPrefix = "";

            float initialStatValue = 0;
            float statValueModifierMultiplier = 1;
            float absoluteStatValueModifier = 0;
            float totalStatWithModifier = 0;
            float relativeStatModifier = 0;

            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxActiveSkillsDisplayed) {
                initialStatValue = StructureStats.Instance.GetInitialSkillMerchantMaxActiveSkillsDisplayed();
                currentStatValue = StructureStats.Instance.GetSkillMerchantMaxActiveSkillsDisplayed().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxPassiveSkillsDisplayed) {
                initialStatValue = StructureStats.Instance.GetInitialSkillMerchantMaxPassiveSkillsDisplayed();
                currentStatValue = StructureStats.Instance.GetSkillMerchantMaxPassiveSkillsDisplayed().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapsDisplayed) {
                initialStatValue = StructureStats.Instance.GetInitialTrapMerchantMaxTrapsDisplayed();
                currentStatValue = StructureStats.Instance.GetTrapMerchantMaxTrapsDisplayed().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapUpgradesDisplayed) {
                initialStatValue = StructureStats.Instance.GetInitialTrapMerchantMaxTrapUpgradesDisplayed();
                currentStatValue = StructureStats.Instance.GetTrapMerchantMaxTrapUpgradesDisplayed().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomTrap) {
                initialStatValue = StructureStats.Instance.GetInitialStartWithRandomTrapAmount();
                currentStatValue = StructureStats.Instance.GetStartWithRandomTrapAmount().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomActiveSkill) {
                initialStatValue = 0;
                currentStatValue = PlayerStats.Instance.GetStartWithRandomActiveSkillLevel().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomPassiveSkill) {
                initialStatValue = 0;
                currentStatValue = PlayerStats.Instance.GetStartWithRandomPassiveSkillLevel().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemLevel == maxItemLevel) {
                absoluteStatValueModifier = linkedStatModifierSO.statModifierList[itemLevel - 1];
                totalStatWithModifier = initialStatValue + absoluteStatValueModifier * statValueModifierMultiplier;
                totalStatValue = totalStatWithModifier.ToString();
            }
            else {
                absoluteStatValueModifier = linkedStatModifierSO.statModifierList[itemLevel];
                totalStatWithModifier = initialStatValue + absoluteStatValueModifier * statValueModifierMultiplier;

                totalStatValue = totalStatWithModifier.ToString();
                relativeStatModifier = linkedStatModifierSO.statModifierList[itemLevel];

                if (itemLevel > 0) {
                    relativeStatModifier = linkedStatModifierSO.statModifierList[itemLevel] - linkedStatModifierSO.statModifierList[itemLevel - 1];
                }
            }

            if (itemLevel == maxItemLevel) {
                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
            else {
                statValues.Add(initialStatPrefix + currentStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(false);

                statValues.Add(relativeStatPrefix + relativeStatModifier.ToString() + relativeStatPostfix);
                statModifiedBools.Add(true);

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
        }
    }
    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        statDescriptionList.Add("");

        if(mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newActiveSkill || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newPassiveSkill || mushroomItemCategory == HubMerchantItem_MushroomMerchantItemCategory.newTrap) {
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_ItemDescription"));
            statDescriptionList.Add("");
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxActiveSkillsDisplayed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxActiveSkillsDisplayed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxActiveSkillsDisplayed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxActiveSkillsDisplayed") + " ");
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.skillsMerchantMaxPassiveSkillsDisplayed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxPassiveSkillsDisplayed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxPassiveSkillsDisplayed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxPassiveSkillsDisplayed") + " ");
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapsDisplayed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxTrapsDisplayed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxTrapsDisplayed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxTrapsDisplayed") + " ");
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.trapMerchantMaxTrapUpgradesDisplayed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxTrapUpgradesDisplayed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxTrapUpgradesDisplayed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxTrapUpgradesDisplayed") + " ");
        }

        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomTrap) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentStartWithTrapAmount") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_startWithTrapAmount") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newStartWithTrapAmount") + " ");
        }
        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomActiveSkill) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentStartWithRandomActiveSkillLevel") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_startWithRandomActiveSkillLevel") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newStartWithRandomActiveSkillLevel") + " ");
        }
        if (mushroomItemType == HubMerchantItem_MushroomMerchantItemType.startWith1RandomPassiveSkill) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentStartWithRandomPassiveSkillLevel") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_startWithRandomPassiveSkillLevel") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newStartWithRandomPassiveSkillLevel") + " ");
        }

        return statDescriptionList;
    }
    public override bool GetConstantUnlockDescription() {
        return false;
    }

    public override string GetItemType() {
        return mushroomItemType.ToString();
    }
}
