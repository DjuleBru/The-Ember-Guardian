using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Steamworks.InventoryItem;

public class HordeModeProgressionManager : MonoBehaviour
{
    public static HordeModeProgressionManager Instance;
    public enum HordeModeUnlockables {
        Barricade2,
        SMG,
        Tower2,
        Shotgun,
        Trainer,
        Sniper,
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
        ArchitectTable,
        EngineerShrine,
        SniperTower,
        LumenHollow,
        SecondaryFire,
        MGTower,
        RL,
        Tent3,
        MortarTower,
        FastTravelTP_WithinBase,
        AAGun,
        OrbExtractor,
        GuardShrine,
        FracturedDistrict,
        AR,
        TrapsMerchant,
        DarkCompanion,
        Tower4_Barricade4,
        Minigun,
        SniperTower2,
        MGTower2,
        MortarTower2
    }
    public Dictionary<HordeModeUnlockables, int> unlockThresholds;

    private int totalHordeModeXP;
    private int pendingXP; // XP gained during this run but not saved yet
    private bool hasXPToCommit = false;

    // Unlock data
    public HashSet<HordeModeUnlockables> unlockedSet = new HashSet<HordeModeUnlockables>();

    private void Awake() {
        Instance = this;

        totalHordeModeXP = ES3.Load("HordeModeXP", 0);
        pendingXP = ES3.Load("HordeModeXP_pending", 0);
        hasXPToCommit = ES3.Load("hasXPToCommit", false);

        unlockThresholds = GenerateUnlockThresholds();
        unlockedSet = ES3.Load("HordeModeUnlocks", new HashSet<HordeModeUnlockables>());
    }

    private Dictionary<HordeModeUnlockables, int> GenerateUnlockThresholds() {
        Dictionary<HordeModeUnlockables, int> dict = new Dictionary<HordeModeUnlockables, int>();

        int baseXP = 100;         // premier seuil
        int incremental = 10;     // augmentation progressive supplémentaire
        int currentIncrease = 0;  // augmente de 0, puis 10, puis 20, etc.

        int currentXP = baseXP;

        foreach (HordeModeUnlockables unlock in System.Enum.GetValues(typeof(HordeModeUnlockables))) {
            dict.Add(unlock, currentXP);

            currentIncrease += incremental;
            currentXP += baseXP + currentIncrease;
        }

        return dict;
    }

    // Called during run
    [Button]
    public void AddRunXP(int amount) {
        pendingXP += amount;
        totalHordeModeXP += amount;

        ES3.Save("HordeModeXP", totalHordeModeXP);
        ES3.Save("HordeModeXP_pending", pendingXP);

        hasXPToCommit = true;
        ES3.Save("hasXPToCommit", true);
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
    }

    private void OnUnlockEarned(HordeModeUnlockables unlock) {
        Debug.Log("Unlocked: " + unlock);
    }

    public bool IsUnlocked(HordeModeUnlockables unlock) {
        return unlockedSet.Contains(unlock);
    }

    public int GetTotalXP() => totalHordeModeXP;

    public int GetPendingXP() => pendingXP;


    public bool GetUnlocked(HordeModeUnlockables unlockable) {
        if (unlockedSet == null)
            return false;

        return unlockedSet.Contains(unlockable);
    }

    public HordeModeUnlockables GetNextUnlockable() {
        foreach (var kvp in unlockThresholds) {
            var unlock = kvp.Key;
            var requiredXP = kvp.Value;

            if (!unlockedSet.Contains(unlock))
                return unlock;
        }

        // Plus rien à débloquer => renvoie un faux élément ou gère le cas différemment
        return (HordeModeUnlockables)(-1);
    }
}
