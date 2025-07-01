using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBMerchantItem_DogTamerItem : HubMerchantItem
{
    public enum DogTamerItemType {
        GermanShepherd_BiteAbility,
        GermanShepherd_DigResourceAbility,
        GermanShepherd_BiteCooldown,
        GermanShepherd_BiteDamage,
        GermanShepherd_DigResourceCooldown,
        GermanShepherd_DigResourceDoubleProbability,
        GermanShepherd_AmbushDetectionProbability,
        GermanShepherd_AmbushDetectionAbility,
        GermanShepherd_DigResourceProbability,

        Dog_GermanShepherd,
        Dog_GoldenRetreiver,
        Dog_DarkCompanion,

        Retreiver_BiteAbility,
        Retreiver_BiteCooldown,
        Retreiver_BiteDamage,
        Retreiver_BuffWorkersAbility,
        Retreiver_BuffWorkersBuffAmount,
        Retreiver_BuffWorkersRadius,
        Retreiver_PickUpItemsAbility,

        DarkCompanion_BiteAbility,
        DarkCompanion_BiteCooldown,
        DarkCompanion_BiteDamage,
        DarkCompanion_LaserAbility,
        DarkCompanion_LaserDamage,
        DarkCompanion_LaserCooldown,
        DarkCompanion_StompAbility,
        DarkCompanion_StompCooldown,
        DarkCompanion_StompStunDuration,
        DarkCompanion_StompDamage,
    }

    public enum DogTamerItemCategory {
        NewAbility,
        StatUpgrade,
        NewDog,
    }

    [SerializeField] private DogTamerItemType itemType;
    [SerializeField] private DogTamerItemCategory itemCategory;

    protected override void Awake() {
        base.Awake();
        RefreshStatValues();
    }

    public override void BuyItem() {
        if (itemCategory == DogTamerItemCategory.NewAbility) {
            if(itemType == DogTamerItemType.GermanShepherd_BiteAbility) {
                DogStats.Instance.UnlockGermanShepherdBiteAbility();
            }
            if(itemType == DogTamerItemType.GermanShepherd_DigResourceAbility) {
                DogStats.Instance.UnlockGermanShepherdDigResourceAbility();
            }
            if(itemType == DogTamerItemType.GermanShepherd_AmbushDetectionAbility) {
                DogStats.Instance.UnlockGermanShepherdDetectAmbushAbility();
            }
            if (itemType == DogTamerItemType.Retreiver_BiteAbility) {
                DogStats.Instance.UnlockRetreiverBiteAbility();
            }
            if (itemType == DogTamerItemType.Retreiver_BuffWorkersAbility) {
                DogStats.Instance.UnlockRetreiverBuffWorkersAbility();
            }
            if (itemType == DogTamerItemType.Retreiver_PickUpItemsAbility) {
                DogStats.Instance.UnlockRetreiverPickUpItemsAbility();
            }
            if (itemType == DogTamerItemType.DarkCompanion_BiteAbility) {
                DogStats.Instance.UnlockDarkCompanionBiteAbility();
            }
            if (itemType == DogTamerItemType.DarkCompanion_LaserAbility) {
                DogStats.Instance.UnlockDarkCompanionLaserAbility();
            }
            if (itemType == DogTamerItemType.DarkCompanion_StompAbility) {
                DogStats.Instance.UnlockDarkCompanionStompAbility();
            }
        }

        if (itemCategory == DogTamerItemCategory.NewDog) {
            Debug.Log(itemType);
            if (itemType == DogTamerItemType.Dog_GoldenRetreiver) {
                DogStats.Instance.UnlockRetreiver();
            }
            if (itemType == DogTamerItemType.Dog_DarkCompanion) {
                Debug.Log("UnlockDarkCompanion");
                DogStats.Instance.UnlockDarkCompanion();
            }
        }

        if (itemCategory == DogTamerItemCategory.StatUpgrade) {
            SetNewStatIncreaseStats();
        }
        

        base.BuyItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    public override void UpgradeItem() {
        if (itemCategory != DogTamerItemCategory.NewAbility) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();

        if (itemCategory != DogTamerItemCategory.NewAbility && itemCategory != DogTamerItemCategory.NewDog) {

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
            float relativeDamageBulletModifier = 0;


            if (itemType == DogTamerItemType.GermanShepherd_BiteCooldown) {
                initialStatValue = DogStats.Instance.GetGermanShepherdInitialBiteCooldown();
                currentStatValue = DogStats.Instance.GetGermanShepherdBiteCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_BiteDamage) {
                initialStatValue = DogStats.Instance.GetInitialGermanShepherdBiteDamage();
                currentStatValue = DogStats.Instance.GetGermanShepherdBiteDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.Retreiver_BiteCooldown) {
                initialStatValue = DogStats.Instance.GetInitialRetreiverBiteCooldown();
                currentStatValue = DogStats.Instance.GetRetreiverBiteCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.Retreiver_BiteDamage) {
                initialStatValue = DogStats.Instance.GetInitialRetreiverBiteDamage();
                currentStatValue = DogStats.Instance.GetRetreiverBiteDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_BiteCooldown) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionBiteCooldown();
                currentStatValue = DogStats.Instance.GetDarkCompanionBiteCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_BiteDamage) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionBiteDamage();
                currentStatValue = DogStats.Instance.GetDarkCompanionBiteDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_LaserCooldown) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionLaserCooldown();
                currentStatValue = DogStats.Instance.GetDarkCompanionLaserCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_LaserDamage) {
                initialStatValue = (int)(DogStats.Instance.GetInitialDarkCompanionLaserDamage() / DogStats.Instance.GetDarkCompanionLaserTickCooldown());
                currentStatValue = ((int)(DogStats.Instance.GetDarkCompanionLaserDamage() / DogStats.Instance.GetDarkCompanionLaserTickCooldown())).ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_StompCooldown) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionStompCooldown();
                currentStatValue = DogStats.Instance.GetDarkCompanionStompCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_StompDamage) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionStompDamage();
                currentStatValue = DogStats.Instance.GetDarkCompanionStompDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.DarkCompanion_StompStunDuration) {
                initialStatValue = DogStats.Instance.GetInitialDarkCompanionStompStunDuration();
                currentStatValue = DogStats.Instance.GetDarkCompanionStompStunDuration().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
                initialStatValue = DogStats.Instance.GetGermanShepherdInitialDigResourceProbability();
                currentStatValue = DogStats.Instance.GetGermanShepherdDigResourceProbility().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
                initialStatValue = DogStats.Instance.GetGermanShepherdInitialDigResourceCooldown();
                currentStatValue = DogStats.Instance.GetGermanShepherdDigResourceCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
                initialStatValue = DogStats.Instance.GetGermanShepherdInitialDigResourceDoubleProbability();
                currentStatValue = DogStats.Instance.GetGermanShepherdDigResourceDoubleProbability().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
                initialStatValue = DogStats.Instance.GetGermanShepherdInitialAmbushDetectionProbability();
                currentStatValue = DogStats.Instance.GetGermanShepherdDetectAmbushProbability().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.Retreiver_BuffWorkersBuffAmount) {
                initialStatValue = DogStats.Instance.GetInitialRetreiverBuffWorkersAmount();
                currentStatValue = DogStats.Instance.GetRetreiverBuffWorkersAmount().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.Retreiver_BuffWorkersRadius) {
                initialStatValue = DogStats.Instance.GetInitialRetreiverBuffWorkersRadius();
                currentStatValue = DogStats.Instance.GetRetreiverBuffWorkersRadius().ToString();
                totalStatWithModifierPostfix = "m";
                relativeStatPostfix = "m";
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
                relativeDamageBulletModifier = linkedStatModifierSO.statModifierList[itemLevel];

                if (itemLevel > 0) {
                    relativeDamageBulletModifier = linkedStatModifierSO.statModifierList[itemLevel] - linkedStatModifierSO.statModifierList[itemLevel - 1];
                }
            }

            if (itemLevel == maxItemLevel) {
                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
            else {
                statValues.Add(initialStatPrefix + currentStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(false);

                statValues.Add(relativeStatPrefix + relativeDamageBulletModifier.ToString() + relativeStatPostfix);
                statModifiedBools.Add(true);

                statValues.Add("");
                statModifiedBools.Add(false);

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }


        }
    }

    private void SetNewStatIncreaseStats() {
        float newAbsoluteValueBuff = linkedStatModifierSO.statModifierList[itemLevel];

        if (itemType == DogTamerItemType.GermanShepherd_BiteCooldown) {
            DogStats.Instance.BuffGermanShepherdBiteCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_BiteDamage) {
            DogStats.Instance.BuffGermanShepherdBiteDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
            DogStats.Instance.BuffGermanShepherdDigResourceDoubleProbability(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
            DogStats.Instance.BuffGermanShepherdDigResourceCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
            DogStats.Instance.BuffGermanShepherdAmbushDetectionProbability(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
            DogStats.Instance.BuffGermanShepherdDigResourceProbability(newAbsoluteValueBuff);
        }

        if (itemType == DogTamerItemType.Retreiver_BiteCooldown) {
            DogStats.Instance.BuffRetreiverBiteCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.Retreiver_BiteDamage) {
            DogStats.Instance.BuffRetreiverBiteDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.Retreiver_BuffWorkersBuffAmount) {
            DogStats.Instance.BuffRetreiverBuffWorkersAmount(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.Retreiver_BuffWorkersRadius) {
            DogStats.Instance.BuffRetreiverBuffWorkersRadius(newAbsoluteValueBuff);
        }

        if (itemType == DogTamerItemType.DarkCompanion_BiteCooldown) {
            DogStats.Instance.BuffDarkCompanionBiteCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_BiteDamage) {
            DogStats.Instance.BuffDarkCompanionBiteDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_LaserCooldown) {
            DogStats.Instance.BuffDarkCompanionLaserCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_LaserDamage) {
            DogStats.Instance.BuffDarkCompanionLaserDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompCooldown) {
            DogStats.Instance.BuffDarkCompanionStompCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompDamage) {
            DogStats.Instance.BuffDarkCompanionStompDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompStunDuration) {
            DogStats.Instance.BuffDarkCompanionStompStunDuration((int)newAbsoluteValueBuff);
        }
    }

    public override bool GetConstantUnlockDescription() {
        bool constantUnlockDescription = false;

        if (itemCategory == DogTamerItemCategory.NewDog) {
            constantUnlockDescription = true;
        }
        if (itemCategory == DogTamerItemCategory.NewAbility) {
            constantUnlockDescription = true;
        }

        return constantUnlockDescription;
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        if (itemCategory == DogTamerItemCategory.NewAbility || itemCategory == DogTamerItemCategory.NewDog) {
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        }

        if (itemType == DogTamerItemType.GermanShepherd_BiteCooldown || itemType == DogTamerItemType.Retreiver_BiteCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentBiteCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_biteCooldown") + " ");
                statDescriptionList.Add("");
            }

            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBiteCooldown") + " ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_BiteDamage || itemType == DogTamerItemType.Retreiver_BiteDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentBiteDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_biteDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBiteDamage") + " ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDigCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_digCooldown") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDigCooldown") + " ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDigProbability") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_digProbability") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDigProbability") + " ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDoubleDigProbability") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_doubleDigProbability") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDoubleDigProbability") + " ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDetectAmbushProbability") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_detectAmbushProbability") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDetectAmbushProbability") + " ");
        }


        if (itemType == DogTamerItemType.Retreiver_BuffWorkersBuffAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRetreiverBuffAmount") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_retreiverBuffAmount") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRetreiverBuffAmount") + " ");
        }
        if (itemType == DogTamerItemType.Retreiver_BuffWorkersRadius) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRetreiverBuffRadius") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_retreiverBuffRadius") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRetreiverBuffRadius") + " ");
        }

        if (itemType == DogTamerItemType.DarkCompanion_BiteCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentLaserShotCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_laserShotCooldown") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newLaserShotCooldown") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_BiteDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentLaserShotDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_laserShotDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newLaserShotDamage") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_LaserCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentChargedBeamCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_chargedBeamCooldown") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newChargedBeamCooldown") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_LaserDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentChargedBeamDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_chargedBeamDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newChargedBeamDamage") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDarkCompanionStompCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_darkCompanionStompCooldown") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDarkCompanionStompCooldown") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDarkCompanionStompDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_darkCompanionStompDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDarkCompanionStompDamage") + " ");
        }
        if (itemType == DogTamerItemType.DarkCompanion_StompStunDuration) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDarkCompanionStompStunDuration") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_darkCompanionStompStunDuration") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDarkCompanionStompStunDuration") + " ");
        }


        return statDescriptionList;
    }

    public override string GetItemType() {
        return itemType.ToString();
    }
}
