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
        RespawnHP
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

        if(trainerItemType != TrainerItemType.Hold2Weapons) {

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

                statValues.Add("");
                statModifiedBools.Add(false);

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

        return constantUnlockDescription;
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        if(trainerItemType == TrainerItemType.Hold2Weapons) {
            statDescriptionList.Add(unlockDescription);
        }

        if (trainerItemType == TrainerItemType.WeaponSwapTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current swap buff ");
                statDescriptionList.Add("Swap buff ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New swap buff ");
        }

        if (trainerItemType == TrainerItemType.Flashlight) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current Range ");
                statDescriptionList.Add("Range increase ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("Flashlight range ");
        }

        if (trainerItemType == TrainerItemType.Crouch) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Detection factor ");
                statDescriptionList.Add("Detection decrease ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New detection factor ");
        }

        if (trainerItemType == TrainerItemType.MaxHP) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current max HP ");
                statDescriptionList.Add("Max HP ");
                statDescriptionList.Add("+");
            }
            statDescriptionList.Add("New max HP ");
        }

        if (trainerItemType == TrainerItemType.RespawnHP) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current respawn HP ");
                statDescriptionList.Add("Respawn HP ");
                statDescriptionList.Add("+");
            }
            statDescriptionList.Add("New respawn HP ");
        }

        if (trainerItemType == TrainerItemType.Heal) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current heal time ");
                statDescriptionList.Add("Time to heal ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("Time to heal ");
        }

        if (trainerItemType == TrainerItemType.MaxStamina) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current max stamina ");
                statDescriptionList.Add("Max stamina ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New max stamina ");
        }

        if (trainerItemType == TrainerItemType.MoveSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current move speed ");
                statDescriptionList.Add("Move speed ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New move speed ");
        }

        if (trainerItemType == TrainerItemType.RunSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current run speed ");
                statDescriptionList.Add("Run speed ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New Run speed ");
        }

        if (trainerItemType == TrainerItemType.RunStaminaCost) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current run cost ");
                statDescriptionList.Add("Run cost ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New run cost ");
        }

        if (trainerItemType == TrainerItemType.RollDistance) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current roll distance ");
                statDescriptionList.Add("Roll distance ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New Roll distance ");
        }

        if (trainerItemType == TrainerItemType.RollStaminaCost) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current roll cost ");
                statDescriptionList.Add("Roll cost ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New roll stamina cost ");
        }

        if (trainerItemType == TrainerItemType.BackpackGemSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Gem pouch size ");
                statDescriptionList.Add("Pouch size ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New pouch size ");
        }

        if (trainerItemType == TrainerItemType.BackpackOrbSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Backpack size ");
                statDescriptionList.Add("Backpack size ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New backpack size ");
        }

        if (trainerItemType == TrainerItemType.BackpackAmmoSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Ammo pouch size ");
                statDescriptionList.Add("Pouch size ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New ammo size ");
        }

        return statDescriptionList;
    }

    public override string GetItemType() {
        return trainerItemType.ToString();
    }
}
