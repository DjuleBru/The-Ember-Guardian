using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_WatcherMerchantItem : HubMerchantItem {
    public enum WatcherItemType {
        ControlEmberlings,
        MaxFollowingWorkers,
        InitialEmberlings,
        EmberlingArrivals,
        HunterShrine,
        HunterSpeed,
        HunterAccuracy,
        HunterDamage,
        HunterAttackCooldown,
        MinerShrine,
        MinerSpeed,
        MinerHealth,
        MinerMiningSpeed,
        MinerLuckyPickaxe,
        GuardShrine,
        GuardSpeed,
        GuardHealth,
        GuardDamage,
        GuardAttackCooldown,
        EngineerShrine,
        EngineerSpeed,
        EngineerWrenchSpeed,
        EngineerContainerSize,
        EngineerContainerUnlock_BigOrb,
        EngineerContainerUnlock_SmallOrb,
        EngineerContainerUnlock_Ammo,
        EngineerContainerUnlock_AmmoSpecial,
    }
    public enum WatcherItemCategory {
        newShrine,
        controlUpgrade,
        statIncrease,
    }

    [SerializeField] private WatcherItemType watcherItemType;
    [SerializeField] private WatcherItemCategory watcherItemCategory;
    [SerializeField] private StructureSO.StructureType structureType;

    protected override void Awake() {
        base.Awake();
        RefreshStatValues();
    }

    public override void BuyItem() {

        if (watcherItemType == WatcherItemType.ControlEmberlings) {

            WorkerStats.Instance.SetInteractionWithWorkersUnlocked();

        } else if(watcherItemCategory != WatcherItemCategory.newShrine) {

            SetNewStatIncreaseStats();

        }

        base.BuyItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }
    public override void UpgradeItem() {
        if (watcherItemType != WatcherItemType.ControlEmberlings && watcherItemCategory != WatcherItemCategory.newShrine) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void SetNewStatIncreaseStats() {
        float buff = linkedStatModifierSO.statModifierList[itemLevel];

        if (watcherItemType == WatcherItemType.MaxFollowingWorkers) {
            WorkerStats.Instance.SetMaxFollowingWorkers((int)buff);
        }
        if (watcherItemType == WatcherItemType.EmberlingArrivals) {
            WorkerStats.Instance.SetEmberlingsArrivalsNumber((int)buff);
        }
        if (watcherItemType == WatcherItemType.InitialEmberlings) {
            WorkerStats.Instance.SetInitialEmberlings((int)buff);
        }

        if (watcherItemType == WatcherItemType.HunterDamage) {
            WorkerStats.Instance.SetHunterDamageBuff((int)buff);
        }
        if (watcherItemType == WatcherItemType.GuardDamage) {
            WorkerStats.Instance.SetGuardDamageBuff((int)buff);
        }

        if (watcherItemType == WatcherItemType.GuardHealth) {
            WorkerStats.Instance.SetGuardHealthBuff((int)buff);
        }
        if (watcherItemType == WatcherItemType.MinerHealth) {
            WorkerStats.Instance.SetMinerHealthBuff((int)buff);
        }

        if (watcherItemType == WatcherItemType.HunterSpeed) {
            WorkerStats.Instance.SetHunterMoveSpeedBuff(buff*0.01f);
        }
        if (watcherItemType == WatcherItemType.MinerSpeed) {
            WorkerStats.Instance.SetMinerMoveSpeedBuff(buff * 0.01f);
        }
        if (watcherItemType == WatcherItemType.GuardSpeed) {
            WorkerStats.Instance.SetGuardMoveSpeedBuff(buff * 0.01f);
        }
        if (watcherItemType == WatcherItemType.EngineerSpeed) {
            WorkerStats.Instance.SetEngineerMoveSpeedBuff(buff * 0.01f);
        }
        if (watcherItemType == WatcherItemType.EngineerWrenchSpeed) {
            WorkerStats.Instance.SetEngineerWrenchSpeedBuff(buff * 0.01f);
        }

        if (watcherItemType == WatcherItemType.HunterAttackCooldown) {
            WorkerStats.Instance.SetHunterAttackCooldownBuff(buff);
        }
        if (watcherItemType == WatcherItemType.MinerMiningSpeed) {
            WorkerStats.Instance.SetMinerAttackCooldownBuff(buff);
        }
        if (watcherItemType == WatcherItemType.GuardAttackCooldown) {
            WorkerStats.Instance.SetGuardAttackCooldownBuff(buff);
        }

        if (watcherItemType == WatcherItemType.HunterAccuracy) {
            WorkerStats.Instance.SetHunterAccuracyBuff(buff);
        }
        if (watcherItemType == WatcherItemType.MinerLuckyPickaxe) {
            WorkerStats.Instance.SetMinerLuckyPickaxeProb(buff * 0.01f);
        }
        if (watcherItemType == WatcherItemType.EngineerContainerSize) {
            StructureStats.Instance.SetEngineerContainerSizeBuff(buff * 0.01f);
        }
    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();
        statModifiedBools.Add(false);
        statValues.Add("");
        statModifiedBools.Add(false);
        statValues.Add("");

        if (watcherItemCategory != WatcherItemCategory.newShrine && watcherItemType != WatcherItemType.ControlEmberlings) {

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


            if (watcherItemType == WatcherItemType.MaxFollowingWorkers) {
                initialStatValue = WorkerStats.Instance.GetInitialMaxFollowingWorkers();
                currentStatValue = WorkerStats.Instance.GetMaxFollowingWorkers().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.InitialEmberlings) {
                initialStatValue = 0;
                currentStatValue = WorkerStats.Instance.GetInitialEmberlings().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.EmberlingArrivals) {
                initialStatValue = 0;
                currentStatValue = WorkerStats.Instance.GetEmberlingsArrivalsNumber().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.HunterSpeed) {
                initialStatValue = 0;
                currentStatValue = (WorkerStats.Instance.GetHunterMoveSpeedBuff()*100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.GuardSpeed) {
                initialStatValue = 0;
                currentStatValue = (WorkerStats.Instance.GetGuardMoveSpeedBuff() * 100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.MinerSpeed) {
                initialStatValue = 0;
                currentStatValue = (WorkerStats.Instance.GetMinerMoveSpeedBuff() * 100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.EngineerSpeed) {
                initialStatValue = 0;
                currentStatValue = (WorkerStats.Instance.GetEngineerMoveSpeedBuff() * 100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.EngineerWrenchSpeed) {
                initialStatValue = 0;
                currentStatValue = (WorkerStats.Instance.GetEngineerWrenchSpeedBuff() * 100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.EngineerContainerSize) {
                initialStatValue = 0;
                currentStatValue = (StructureStats.Instance.GetEngineerContainerSizeBuff() * 100f).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "+";
                totalStatWithModifierPrefix = "+";
            }

            if (watcherItemType == WatcherItemType.HunterDamage) {
                initialStatValue = WorkerStats.Instance.GetInitialHunterDamage();
                currentStatValue = WorkerStats.Instance.GetHunterDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (watcherItemType == WatcherItemType.GuardDamage) {
                initialStatValue = WorkerStats.Instance.GetInitialGuardDamage();
                currentStatValue = WorkerStats.Instance.GetGuardDamage().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.HunterAttackCooldown) {
                initialStatValue = WorkerStats.Instance.GetInitialHunterAttackCooldown();
                currentStatValue = WorkerStats.Instance.GetHunterAttackCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (watcherItemType == WatcherItemType.GuardAttackCooldown) {
                initialStatValue = WorkerStats.Instance.GetInitialGuardAttackCooldown();
                currentStatValue = WorkerStats.Instance.GetGuardAttackCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (watcherItemType == WatcherItemType.MinerMiningSpeed) {
                initialStatValue = WorkerStats.Instance.GetInitialMinerAttackCooldown();
                currentStatValue = WorkerStats.Instance.GetMinerAttackCooldown().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.MinerHealth) {
                initialStatValue = WorkerStats.Instance.GetInitialMinerHealth();
                currentStatValue = WorkerStats.Instance.GetMinerHealth().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (watcherItemType == WatcherItemType.GuardHealth) {
                initialStatValue = WorkerStats.Instance.GetInitialGuardHealth();
                currentStatValue = WorkerStats.Instance.GetGuardHealth().ToString();
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (watcherItemType == WatcherItemType.MinerLuckyPickaxe) {
                initialStatValue = WorkerStats.Instance.GetInitialMinerLuckyPickaxeProb();
                currentStatValue = Mathf.RoundToInt((WorkerStats.Instance.GetMinerLuckyPickaxeProb()*100f)).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (watcherItemType == WatcherItemType.HunterAccuracy) {
                initialStatValue = WorkerStats.Instance.GetInitialHunterAccuracyBuff();
                currentStatValue = WorkerStats.Instance.GetHunterAccuracyBuff().ToString();
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

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
        }
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        statDescriptionList.Add("");

        if (watcherItemType == WatcherItemType.MaxFollowingWorkers) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxFollowingWorkers") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxFollowingWorkers") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxFollowingWorkers") + " ");
        }
        if (watcherItemType == WatcherItemType.InitialEmberlings) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentInitialEmberlingss") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_initialEmberlingss") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newInitialEmberlingss") + " ");
        }
        if (watcherItemType == WatcherItemType.EmberlingArrivals) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentEmberlingArrivals") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_emberlingArrivals") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newEmberlingArrivals") + " ");
        }

        if (watcherItemType == WatcherItemType.HunterDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentHunterDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_hunterDamage") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxHunterDamage") + " ");
        }
        if (watcherItemType == WatcherItemType.GuardDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentGuardDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_guardDamage") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxGuardDamage") + " ");
        }

        if (watcherItemType == WatcherItemType.HunterAttackCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentHunterAttackCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_hunterAttackCooldown") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newHunterAttackCooldown") + " ");
        }
        if (watcherItemType == WatcherItemType.GuardAttackCooldown) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentGuardAttackCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_guardAttackCooldown") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newGuardAttackCooldown") + " ");
        }
        if (watcherItemType == WatcherItemType.MinerMiningSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMinerMiningCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_minerMiningCooldown") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMinerMiningCooldown") + " ");
        }

        if (watcherItemType == WatcherItemType.HunterSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentHunterSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_hunterSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newHunterSpeed") + " ");
        }
        if (watcherItemType == WatcherItemType.GuardSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentGuardSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_guardSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newGuardSpeed") + " ");
        }
        if (watcherItemType == WatcherItemType.MinerSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMinerSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_minerSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMinerSpeed") + " ");
        }
        if (watcherItemType == WatcherItemType.MinerLuckyPickaxe) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMinerLuckyPickaxe") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_minerLuckyPickaxe") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMinerLuckyPickaxe") + " ");
        }
        if (watcherItemType == WatcherItemType.HunterAccuracy) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentHunterAccuracy") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_hunterAccuracy") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newHunterAccuracy") + " ");
        }

        if (watcherItemType == WatcherItemType.EngineerSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentEngineerSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_engineerSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newEngineerSpeed") + " ");
        }
        if (watcherItemType == WatcherItemType.EngineerWrenchSpeed) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentEngineerWrenchSpeed") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_engineerWrenchSpeed") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newEngineerWrenchSpeed") + " ");
        }

        if (watcherItemType == WatcherItemType.EngineerContainerSize) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentEngineerContainerSize") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_engineerContainerSize") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newEngineerContainerSized") + " ");
        }


        return statDescriptionList;
    }
    public override bool GetConstantUnlockDescription() {
        return false;
    }
    public override string GetItemType() {

        if(watcherItemCategory != WatcherItemCategory.newShrine) {
            return watcherItemType.ToString();
        } else {
            return structureType.ToString() + "1";
        }

    }

    public override void SetNewItemUnlocked(bool unlocked) {
        base.SetNewItemUnlocked(unlocked);

        if (unlocked && itemBought) {
            if (watcherItemType == WatcherItemType.MinerShrine || watcherItemType == WatcherItemType.EngineerShrine) {
                InvokeOnAnyHubMerchantItemArchitectTableUnlocks();
            }
        }
    }

    public StructureSO.StructureType GetStructureType() {
        return structureType;
    }
}
