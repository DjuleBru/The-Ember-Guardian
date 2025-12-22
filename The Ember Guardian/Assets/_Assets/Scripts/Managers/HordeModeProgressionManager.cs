using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Steamworks.InventoryItem;

public class HordeModeProgressionManager : MonoBehaviour
{
    public static HordeModeProgressionManager Instance;
    public enum HordeModeUnlockables {
        Barricade2_Tower2,
        SMG,
        Shotgun,
        Trainer,
        Sniper,
        MinerShrine,
        Tamer,
        Revolver,
        OrbProcessor,
        ObservationTower,
        Tent2,
        LMG,
        Watcher,
        CorruptedCity,
        GoldenRetreiver,
        OrbAlchemist,
        GL,
        Architect,
        Barricade3_Tower3,
        Pistol,
        NewSkills1,
        ArchitectTable,
        EngineerShrine,
        SniperTower,
        LumenHollow,
        SecondaryFire,
        MGTower,
        NewSkills2,
        RL,
        Tent3,
        OrbContainers,
        MortarTower,
        FastTravelTP_WithinBase,
        AAGun,
        OrbExtractor,
        GuardShrine,
        FracturedDistrict,
        NewSkills3,
        AR,
        TrapsMerchant,
        DarkCompanion,
        Tower4_Barricade4,
        NewTraps1,
        Minigun,
        NewSkills4,
        SniperTower2,
        MGTower2,
        MortarTower2,
        NewTraps2,
        Armorer,
        MoreCampCustomizationBudget1,
        MoreCampCustomizationBudget2,
        None,
    }
    public Dictionary<HordeModeUnlockables, int> unlockThresholds;

    [SerializeField] private List<LevelSO> levelSOList;

    [Title("Unlock Order")]
    [InfoBox("This list defines the EXACT unlock order. The enum order is ignored.")]
    public List<HordeModeUnlockables> unlockOrder = new List<HordeModeUnlockables>();

    private int totalHordeModeXP;
    private int pendingXP; // XP gained during this run but not saved yet
    private bool hasXPToCommit = false;
    public event EventHandler OnHordeModeUnlockableUnlocked;

    // Unlock data
    public List<HordeModeUnlockables> unlockedSet = new List<HordeModeUnlockables>();

    private void Awake() {
        Instance = this;

        totalHordeModeXP = ES3.Load("HordeModeXP", 0);
        pendingXP = ES3.Load("HordeModeXP_pending", 0);
        hasXPToCommit = ES3.Load("hasXPToCommit", false);

        unlockThresholds = GenerateUnlockThresholds();
        Debug.Log(unlockThresholds[HordeModeUnlockables.Armorer]);
        unlockedSet = ES3.Load("HordeModeUnlocks", new List<HordeModeUnlockables>());
    }

    private void Start() {
        CheckLastMainGameLevelCompleted();
    }

    private Dictionary<HordeModeUnlockables, int> GenerateUnlockThresholds() {
        Debug.Log("GenerateUnlockThresholds");
        Dictionary<HordeModeUnlockables, int> dict = new Dictionary<HordeModeUnlockables, int>();

        int baseXP = 10;
        int incremental = 3;
        int currentIncrease = 0;
        int currentXP = baseXP;

        foreach (var unlock in unlockOrder) {
            if (unlock == HordeModeUnlockables.None)
                continue;

            dict[unlock] = currentXP;

            currentIncrease += incremental;
            currentXP += baseXP + currentIncrease;
        }

        return dict;
    }

    // Called during run
    [Button]
    public void AddRunXP(int amount) {
        Debug.Log("AddRunXP " + amount);
        pendingXP += amount;
        totalHordeModeXP += amount;

        ES3.Save("HordeModeXP", totalHordeModeXP);
        ES3.Save("HordeModeXP_pending", pendingXP);

        hasXPToCommit = true;
        ES3.Save("hasXPToCommit", true);
    }

    private void CheckLastMainGameLevelCompleted() {
        foreach (LevelSO levelSO in levelSOList) {
            if (MetaProgressionManager.Instance.GetLevelCompleted(levelSO)) {
                EnsureUnlockReached_MainGame(levelSO.linkedHordeUnlock);
            }
        }
    }

    public void EnsureUnlockReached_MainGame(HordeModeUnlockables unlock) {
        if (unlock == HordeModeUnlockables.None) return;
        //Debug.Log("GetUnlocked " + unlock  + " " + GetUnlocked(unlock));
        if (GetUnlocked(unlock)) return;

        if (unlockThresholds == null) unlockThresholds = GenerateUnlockThresholds();

        int threshold = unlockThresholds[unlock];
        int needed = threshold - totalHordeModeXP;
        AddRunXP(needed);

        if(needed > 0) {
            ES3.Save("lastXPGainFromMainGame", true);
        }
    }

    public bool GetHasXPToCommit() {
        return hasXPToCommit;
    }

    public void SetHasNoXPToCommit() {
        hasXPToCommit = false;
        ES3.Save("hasXPToCommit", false);

        pendingXP = 0;
        ES3.Save("HordeModeXP_pending", pendingXP);
    }

    public void AddUnlocked(HordeModeUnlockables unlock) {
        unlockedSet.Add(unlock);
        ES3.Save("HordeModeUnlocks", new List<HordeModeProgressionManager.HordeModeUnlockables>(HordeModeProgressionManager.Instance.unlockedSet));
        OnHordeModeUnlockableUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public int GetTotalXP() => totalHordeModeXP;

    public int GetPendingXP() => pendingXP;

    public bool GetUnlocked(HordeModeUnlockables unlockable) {
        if (unlockedSet == null)
            return false;

        if (DebugManager.Instance.GetHordeModeAllUnlockedDebug()) return true;
        return unlockedSet.Contains(unlockable);
    }

    public bool GetStructureUnlocked(StructureSO.StructureType structureType) {
        HordeModeUnlockables unlockable = HordeModeUnlockables.ObservationTower;

        switch (structureType) {

            case StructureSO.StructureType.engineerShrine:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.EngineerShrine;
                break;
            case StructureSO.StructureType.minerShrine:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.MinerShrine;
                break;
            case StructureSO.StructureType.guardShrine:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.GuardShrine;
                break;
            case StructureSO.StructureType.sniperTower:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.SniperTower;
                break;
            case StructureSO.StructureType.mortarTower:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.MortarTower;
                break;
            case StructureSO.StructureType.machineGunTower:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.MGTower;
                break;
            case StructureSO.StructureType.currencyStorage_BigOrb:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbContainers;
                break;
            case StructureSO.StructureType.currencyStorage_SmallOrb:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbContainers;
                break;
            case StructureSO.StructureType.currencyStorage_Ammo:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbContainers;
                break;
            case StructureSO.StructureType.currencyStorage_SpecialAmmo:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbContainers;
                break;
            case StructureSO.StructureType.fastTravelTeleporter:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.FastTravelTP_WithinBase;
                break;
            case StructureSO.StructureType.orbExtractor:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbExtractor;
                break;
            case StructureSO.StructureType.merchant_traps:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.TrapsMerchant;
                break;
            case StructureSO.StructureType.merchant_skills:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.OrbAlchemist;
                break;
            case StructureSO.StructureType.observationTower:
                unlockable = HordeModeProgressionManager.HordeModeUnlockables.ObservationTower;
                break;
        }

        return GetUnlocked(unlockable);
    }

    public bool GetWeaponUnlocked(GunSO.GunType gunType) {
        HordeModeUnlockables gunUnlockable = HordeModeUnlockables.SMG;

        if (gunType == GunSO.GunType.Shotgun) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Shotgun;
        }
        if (gunType == GunSO.GunType.Revolver) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Revolver;
        }
        if (gunType == GunSO.GunType.AssaultRifle) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.AR;
        }
        if (gunType == GunSO.GunType.GrenadeLauncher) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.GL;
        }
        if (gunType == GunSO.GunType.AAGun) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.AAGun;
        }
        if (gunType == GunSO.GunType.Pistol) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Pistol;
        }
        if (gunType == GunSO.GunType.MiniGun) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Minigun;
        }
        if (gunType == GunSO.GunType.RocketLauncher) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.RL;
        }
        if (gunType == GunSO.GunType.Sniper) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Sniper;
        }
        if (gunType == GunSO.GunType.LMG) {
            gunUnlockable = HordeModeProgressionManager.HordeModeUnlockables.LMG;
        }

        return GetUnlocked(gunUnlockable);
    }

    public bool GetSkillUnlocked(SkillSO skillSO) {
        if(skillSO.skillType == SkillItem.SkillType.activeMagmaShotBullet || skillSO.skillType == SkillItem.SkillType.activeHealOnKills || skillSO.skillType == SkillItem.SkillType.passiveShieldGenerator || skillSO.skillType == SkillItem.SkillType.passiveDmgIncreaseInLight) {
            return GetUnlocked(HordeModeUnlockables.NewSkills1);
        }
        if (skillSO.skillType == SkillItem.SkillType.activeDarkFlame || skillSO.skillType == SkillItem.SkillType.activeWorkerAttackSpeedBuff || skillSO.skillType == SkillItem.SkillType.passiveAmmoGenerator || skillSO.skillType == SkillItem.SkillType.passiveShootOnReload) {
            return GetUnlocked(HordeModeUnlockables.NewSkills2);
        }
        if (skillSO.skillType == SkillItem.SkillType.activeDarkSword || skillSO.skillType == SkillItem.SkillType.activeFeedFireOnKills || skillSO.skillType == SkillItem.SkillType.passiveMeleeAttackMagmaShot || skillSO.skillType == SkillItem.SkillType.passiveChanceToDoubleXPDrop) {
            return GetUnlocked(HordeModeUnlockables.NewSkills3);
        }
        if (skillSO.skillType == SkillItem.SkillType.activeReaper || skillSO.skillType == SkillItem.SkillType.activePlantMine || skillSO.skillType == SkillItem.SkillType.passiveDmgIncreaseNotInLight || skillSO.skillType == SkillItem.SkillType.passiveLastBulletDealsTwiceDamage) {
            return GetUnlocked(HordeModeUnlockables.NewSkills4);
        }

        return true;
    }

    public bool GetTrapUnlocked(TrapSO trapSO) {
        if(trapSO.trapType == TrapItem.TrapType.bladeTrap || trapSO.trapType == TrapItem.TrapType.smokeEjector || trapSO.trapType == TrapItem.TrapType.shockerEjector) {
            return GetUnlocked(HordeModeUnlockables.NewTraps1);
        }
        if (trapSO.trapType == TrapItem.TrapType.bearTrap || trapSO.trapType == TrapItem.TrapType.fireEjector) {
            return GetUnlocked(HordeModeUnlockables.NewTraps2);
        }

        return true;
    }

    public bool GetMerchantUnlocked(HubMerchant.HubMerchantType merchantType) {
        if(merchantType == HubMerchant.HubMerchantType.HeroMerchant) {
            return GetUnlocked(HordeModeUnlockables.Trainer);
        }
        if (merchantType == HubMerchant.HubMerchantType.GunMerchant) {
            return GetUnlocked(HordeModeUnlockables.Armorer);
        }
        if (merchantType == HubMerchant.HubMerchantType.DogTamer) {
            return GetUnlocked(HordeModeUnlockables.Tamer);
        }
        if (merchantType == HubMerchant.HubMerchantType.StructuresMerchant) {
            return GetUnlocked(HordeModeUnlockables.Architect);
        }
        if (merchantType == HubMerchant.HubMerchantType.WorkerMerchant) {
            return GetUnlocked(HordeModeUnlockables.Watcher);
        }
        return false;
    }

    public List<TrapSO> GetTrapUnlockedList() {
        List<TrapSO> allTrapsList = TrapManager.Instance.GetAllTrapSOList();
        List<TrapSO> allTrapsUnlockedList = new List<TrapSO>();

        foreach(TrapSO trapSO in allTrapsList) {
            if(GetTrapUnlocked(trapSO)) {
                allTrapsUnlockedList.Add(trapSO);
            }
        }

        return allTrapsUnlockedList;
    }

    public int GetWeaponAmountUnlocked() {
        List<HordeModeUnlockables> allWeaponUnlockables = new List<HordeModeUnlockables> {
            HordeModeUnlockables.SMG,
            HordeModeUnlockables.Shotgun,
            HordeModeUnlockables.Revolver,
            HordeModeUnlockables.AR,
            HordeModeUnlockables.GL,
            HordeModeUnlockables.AAGun,
            HordeModeUnlockables.Pistol,
            HordeModeUnlockables.Minigun,
            HordeModeUnlockables.RL,
            HordeModeUnlockables.Sniper,
            HordeModeUnlockables.LMG,
        };

        int gunsUnlocked = 0;
        foreach(HordeModeUnlockables unlockable in allWeaponUnlockables) {
            if (GetUnlocked(unlockable)) {
                gunsUnlocked++;
            }
        }
        return gunsUnlocked;
    }

    public bool LastXPGainWasFromMainGame() {
        return ES3.Load("lastXPGainFromMainGame", false);
    }

    public HordeModeUnlockables GetNextUnlockable() {
        foreach (var unlock in unlockOrder) {
            if (unlock == HordeModeUnlockables.None)
                continue;

            if (!unlockedSet.Contains(unlock))
                return unlock;
        }

        return HordeModeUnlockables.None;
    }
    public bool GetAllUnlocked() {

        foreach (var unlock in unlockOrder) {
            if (unlock == HordeModeUnlockables.None)
                continue;

            if (!unlockedSet.Contains(unlock))
                return false;
        }

        return true;
    }
    public HordeModeUnlockables GetPreviousUnlockable() {
        HordeModeUnlockables previous = HordeModeUnlockables.None;

        foreach (var unlock in unlockOrder) {
            if (unlock == HordeModeUnlockables.None)
                continue;

            if (!unlockedSet.Contains(unlock))
                return previous;

            previous = unlock;
        }

        // Tout débloqué = dernier de la liste
        return previous;
    }

}
