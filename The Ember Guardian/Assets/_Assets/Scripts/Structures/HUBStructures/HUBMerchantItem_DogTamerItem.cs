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

        GoldenRetreiver_BiteAbility,
        GoldenRetreiver_BiteCooldown,
        GoldenRetreiver_BiteDamage,
        GoldenRetreiver_BuffWorkersAbility,
        GoldenRetreiver_BuffWorkersBuffAmount,
        GoldenRetreiver_BuffWorkersRadius,
        GoldenRetreiver_PickUpItems,

        DarkCompanion_BiteAbility,
        DarkCompanion_BiteCooldown,
        DarkCompanion_BiteDamage,
        DarkCompanion_LaserAttack,
        DarkCompanion_LaserDamagePerSecond,
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
                DogStats.Instance.UnlockBiteAbility();
            }
            if(itemType == DogTamerItemType.GermanShepherd_DigResourceAbility) {
                DogStats.Instance.UnlockDigResourceAbility();
            }
            if(itemType == DogTamerItemType.GermanShepherd_AmbushDetectionAbility) {
                DogStats.Instance.UnlockDetectAmbushAbility();
            }
        }
        else {
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

        if (itemCategory != DogTamerItemCategory.NewAbility) {

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
                initialStatValue = DogStats.Instance.GetInitialBiteCooldown();
                currentStatValue = DogStats.Instance.GetBiteCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_BiteDamage) {
                initialStatValue = DogStats.Instance.GetInitialBiteDamage();
                currentStatValue = DogStats.Instance.GetBiteDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
                initialStatValue = DogStats.Instance.GetInitialDigResourceProbability();
                currentStatValue = DogStats.Instance.GetDigResourceProbility().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
                initialStatValue = DogStats.Instance.GetInitialDigResourceCooldown();
                currentStatValue = DogStats.Instance.GetDigResourceCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
                initialStatValue = DogStats.Instance.GetInitialDigResourceDoubleProbability();
                currentStatValue = DogStats.Instance.GetDigResourceDoubleProbability().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
                initialStatValue = DogStats.Instance.GetInitialAmbushDetectionProbability();
                currentStatValue = DogStats.Instance.GetDetectAmbushProbability().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
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
            DogStats.Instance.BuffBiteCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_BiteDamage) {
            DogStats.Instance.BuffBiteDamage((int)newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
            DogStats.Instance.BuffDigResourceDoubleProbability(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
            DogStats.Instance.BuffDigResourceCooldown(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
            DogStats.Instance.BuffAmbushDetectionProbability(newAbsoluteValueBuff);
        }
        if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
            DogStats.Instance.BuffDigResourceProbability(newAbsoluteValueBuff);
        }
    }

    public override bool GetConstantUnlockDescription() {
        bool constantUnlockDescription = false;

        if (itemCategory == DogTamerItemCategory.NewAbility) {
            constantUnlockDescription = true;
        }

        return constantUnlockDescription;
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        if (itemCategory == DogTamerItemCategory.NewAbility) {
            statDescriptionList.Add(unlockDescription);
        }

        if (itemType == DogTamerItemType.GermanShepherd_BiteCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current bite cooldown ");
                statDescriptionList.Add("Bite cooldown ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New bite cooldown ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_BiteDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current bite damage ");
                statDescriptionList.Add("Bite damage ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New bite damage ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current dig cooldown ");
                statDescriptionList.Add("Dig cooldown ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New dig cooldown ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current dig probability ");
                statDescriptionList.Add("Dig probability ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New dig probability ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_DigResourceDoubleProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Dig x2 chance ");
                statDescriptionList.Add("x2 chance ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New x2 chance ");
        }

        if (itemType == DogTamerItemType.GermanShepherd_AmbushDetectionProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Detect ambush chance ");
                statDescriptionList.Add("Detection chance ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New detect chance ");
        }
        return statDescriptionList;
    }

    public override string GetItemType() {
        return itemType.ToString();
    }
}
