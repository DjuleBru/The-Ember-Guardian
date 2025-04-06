using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_TrainerMerchantItem : HubMerchantItem
{
    public enum TrainerItemType {
        Hold2Weapons,
        WeaponSwapTime,
        Flashlight,
        Crouch,
        MaxHP,
        Heal,
        MoveSpeed,
        RunSpeed,
        RunStaminaCost,
        BackpackGemSize,
        BackpackOrbSize,
        BackpackAmmoSize,
        RollDistance,
        RollStaminaCost,
        MaxStamina,
        RespawnHP,
        InitialAmmo,
        InitialOrbs,
    }

    [SerializeField] private TrainerItemType trainerItemType;

    protected override void Awake() {
        base.Awake();
        RefreshStatValues();
    }

    public override void BuyItem() {
        if(trainerItemType == TrainerItemType.Hold2Weapons) {
            PlayerStats.Instance.UnlockCanHold2Weapons();
        } else {
            SetNewStatIncreaseStats();
        }

        base.BuyItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    public override void UpgradeItem() {
        if (trainerItemType != TrainerItemType.Hold2Weapons) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();
        statModifiedBools.Add(false);
        statValues.Add("");
        statModifiedBools.Add(false);
        statValues.Add("");

        if (trainerItemType != TrainerItemType.Hold2Weapons) {

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


            if (trainerItemType == TrainerItemType.WeaponSwapTime) {
                initialStatValue = PlayerStats.Instance.GetInitialSwapWeaponTimeReductionPercent();
                currentStatValue = PlayerStats.Instance.GetSwapWeaponTimeReductionPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.Flashlight) {
                initialStatValue = PlayerStats.Instance.GetInitialFlashlightRange();
                currentStatValue = PlayerStats.Instance.GetFlashlightRange().ToString();
                totalStatWithModifierPostfix = "m";
                relativeStatPostfix = "m";
                relativeStatPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.Crouch) {
                initialStatValue = PlayerStats.Instance.GetInitialCrouchDetectionRangeReductionFactor();
                currentStatValue = PlayerStats.Instance.GetCrouchDetectionRangeReductionPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "-";
                totalStatWithModifierPrefix = "-";
            }

            if (trainerItemType == TrainerItemType.MaxHP) {
                initialStatValue = PlayerStats.Instance.GetInitialPlayerMaxHP();
                currentStatValue = PlayerStats.Instance.GetMaxHP().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.RespawnHP) {
                initialStatValue = PlayerStats.Instance.GetInitialPlayerRespawnHP();
                currentStatValue = PlayerStats.Instance.GetPlayerRespawnHP().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
            }
            if (trainerItemType == TrainerItemType.InitialAmmo) {
                initialStatValue = PlayerStats.Instance.GetInitialStartLevelAmmo();
                currentStatValue = PlayerStats.Instance.GetStartLevelAmmo().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.InitialOrbs) {
                initialStatValue = PlayerStats.Instance.GetInitialStartLevelOrbs();
                currentStatValue = PlayerStats.Instance.GetStartLevelOrbs().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.Heal) {
                initialStatValue = PlayerStats.Instance.GetInitialHpRegenTimer();
                currentStatValue = PlayerStats.Instance.GetHpRegenTime().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
            }

            if (trainerItemType == TrainerItemType.MaxStamina) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetMaxStaminaPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.MoveSpeed) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetMoveSpeedPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.RunSpeed) {
                initialStatValue = 20;
                currentStatValue = (initialStatValue + PlayerStats.Instance.GetRunAccelerationFactorBuff_Meta()).ToString(); ;
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
            }

            if (trainerItemType == TrainerItemType.RunStaminaCost) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetRunStaminaDepletionPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "-";
                totalStatWithModifierPrefix = "-";
            }

            if (trainerItemType == TrainerItemType.RollDistance) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetRollForcePercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.RollStaminaCost) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetRollStaminaDepletionPercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "-";
                initialStatPrefix = "-";
                totalStatWithModifierPrefix = "-";
            }

            if (trainerItemType == TrainerItemType.BackpackGemSize) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetBackpackGemSizePercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.BackpackAmmoSize) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetBackpackAmmoSizePercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (trainerItemType == TrainerItemType.BackpackOrbSize) {
                initialStatValue = 0f;
                currentStatValue = PlayerStats.Instance.GetBackpackOrbSizePercentBuff_Meta().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
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

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }


        }
    }

    private void SetNewStatIncreaseStats() {
        float buff = linkedStatModifierSO.statModifierList[itemLevel];

        if (trainerItemType == TrainerItemType.WeaponSwapTime) {
            PlayerStats.Instance.SetSwapWeaponTimeReductionPercent(buff);
        }

        if (trainerItemType == TrainerItemType.Flashlight) {
            PlayerStats.Instance.SetFlashlightRangeBuff((int)buff);
        }

        if (trainerItemType == TrainerItemType.Crouch) {
            PlayerStats.Instance.SetCrouchDetectionRangeBuff(buff);
        }

        if (trainerItemType == TrainerItemType.MaxHP) {
            PlayerStats.Instance.SetMaxPlayerHPBuff((int)buff);
        }

        if (trainerItemType == TrainerItemType.RespawnHP) {
            PlayerStats.Instance.SetRespawnPlayerHPBuff((int)buff);
        }

        if (trainerItemType == TrainerItemType.InitialAmmo) {
            PlayerStats.Instance.SetInitialStartLevelAmmoBuff((int)buff);
        }

        if (trainerItemType == TrainerItemType.InitialOrbs) {
            PlayerStats.Instance.SetInitialStartLevelOrbsBuff((int)buff);
        }

        if (trainerItemType == TrainerItemType.Heal) {
            PlayerStats.Instance.SetHpRegenTimeAbsolute(buff);
        }

        if (trainerItemType == TrainerItemType.MaxStamina) {
            PlayerStats.Instance.SetMaxStaminaBuff(buff);
        }

        if (trainerItemType == TrainerItemType.MoveSpeed) {
            PlayerStats.Instance.SetMoveSpeedBuff(buff);
        }

        if (trainerItemType == TrainerItemType.RunSpeed) {
            PlayerStats.Instance.SetRunAccelerationFactorBuff(buff);
        }

        if (trainerItemType == TrainerItemType.RunStaminaCost) {
            Debug.Log("RunStaminaCost " + buff);
            PlayerStats.Instance.SetRunStaminaCostBuff(buff);
        }

        if (trainerItemType == TrainerItemType.RollDistance) {
            PlayerStats.Instance.SetRollForceBuff(buff);
        }

        if (trainerItemType == TrainerItemType.RollStaminaCost) {
            PlayerStats.Instance.SetRollStaminaCostBuff(buff);
        }

        if (trainerItemType == TrainerItemType.BackpackGemSize) {
            PlayerStats.Instance.SetBackpackGemSizeBuff(buff);
        }

        if (trainerItemType == TrainerItemType.BackpackAmmoSize) {
            PlayerStats.Instance.SetBackpackAmmoSizeBuff(buff);
        }

        if (trainerItemType == TrainerItemType.BackpackOrbSize) {
            PlayerStats.Instance.SetBackpackOrbSizeBuff(buff);
        }
    }

    public override bool GetConstantUnlockDescription() {
        bool constantUnlockDescription = false;

        if (trainerItemType == TrainerItemType.Hold2Weapons) {
            constantUnlockDescription = true;
        }

        return false;
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        statDescriptionList.Add("");

        if (trainerItemType == TrainerItemType.WeaponSwapTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_swapWeapons") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_swapBuff") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSwapBuff") + " ");
        }

        if (trainerItemType == TrainerItemType.Flashlight) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRange") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_rangeIncrease") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_flashlightRange") + " ");
        }

        if (trainerItemType == TrainerItemType.Crouch) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_detectionFactor") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_detectionFactorDecrease") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newDetectionFactor") + " ");
        }

        if (trainerItemType == TrainerItemType.MaxHP) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxHP") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxHP") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxHP") + " ");
        }

        if (trainerItemType == TrainerItemType.RespawnHP) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRespawnHP") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_respawnHP") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRespawnHP") + " ");
        }
        if (trainerItemType == TrainerItemType.InitialAmmo) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentInitialAmmo") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_initialAmmo") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newInitialAmmo") + " ");
        }
        if (trainerItemType == TrainerItemType.InitialOrbs) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentInitialOrbs") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_initialOrbs") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newInitialOrbs") + " ");
        }

        if (trainerItemType == TrainerItemType.Heal) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentHealTime") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_healTime") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newHealTime") + " ");
        }

        if (trainerItemType == TrainerItemType.MaxStamina) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxStamina") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxStamina") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxStamina") + " ");
        }

        if (trainerItemType == TrainerItemType.MoveSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMoveSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_moveSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMoveSpeed") + " ");
        }

        if (trainerItemType == TrainerItemType.RunSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRunSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_runSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRunSpeed") + " ");
        }

        if (trainerItemType == TrainerItemType.RunStaminaCost) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRunCost") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_runCost") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRunCost") + " ");
        }

        if (trainerItemType == TrainerItemType.RollDistance) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRollDistance") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_rollDistance") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRollDistance") + " ");
        }

        if (trainerItemType == TrainerItemType.RollStaminaCost) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRollCost") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_rollCost") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRollCost") + " ");
        }

        if (trainerItemType == TrainerItemType.BackpackGemSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentGemPouchSize") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_gemPouchSize") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newGemPouchSize") + " ");
        }

        if (trainerItemType == TrainerItemType.BackpackOrbSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentBackpackSize") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_backpackSize") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBackpackSize") + " ");
        }

        if (trainerItemType == TrainerItemType.BackpackAmmoSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentAmmoPouchSize") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_ammoPouchSize") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newAmmoPouchSize") + " ");
        }

        return statDescriptionList;
    }

    public override string GetItemType() {
        return trainerItemType.ToString();
    }
}
