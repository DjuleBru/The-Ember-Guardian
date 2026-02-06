using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;

public class AchievementsManager : MonoBehaviour {
    public static AchievementsManager Instance;
    private bool playerConnected;

    private void Start() {
        // Singleton strict
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        try {
            SteamClient.Init(3570060);
            Debug.Log(SteamClient.Name);
            playerConnected = true;
        }
        catch (System.Exception e) {
            playerConnected = false;
        }
    }

    private void Update() {
        if (!playerConnected) return;
        SteamClient.RunCallbacks();
    }

    private void OnDestroy() {
        // Sécurité : shutdown propre si l'objet racine est détruit
        if (Instance == this && SteamClient.IsValid) {
            SteamClient.Shutdown();
        }
    }

    public void ImportAchievementsFromSave() {
        if (!playerConnected) return;

        Debug.Log("[Achievements] Import complet depuis save démo");

        // =========================
        // EVENT / ONE-SHOT
        // =========================

        TryUnlockIfTrue("FIRST_PRIMORDIAL_FIRE_LIT",
            ES3.Load("firstPrimordialFireLitAchieved", false));

        TryUnlockIfTrue("KILL_CREATURES_WITHOUT_MOVING",
            ES3.Load("killCreaturesWithoutMovingAchieved", false));

        TryUnlockIfTrue("EMBERLING_AMOUNT",
            ES3.Load("emberlingAmountAchieved", false));

        TryUnlockIfTrue("FIRST_NEST_DESTROYED",
            ES3.Load("firstNestDestroyedAchieved", false));

        TryUnlockIfTrue("FIRST_KILL_DOG",
            ES3.Load("firstKillDogAchieved", false));

        TryUnlockIfTrue("RICOCHET",
            ES3.Load("ricochetAchieved", false));

        TryUnlockIfTrue("PET_DOG",
            ES3.Load("petDogAchieved", false));

        TryUnlockIfTrue("DOG_WAIT_CAMP",
            ES3.Load("dogWaitCampAchieved", false));

        TryUnlockIfTrue("NO_SHOT_FIRED",
            ES3.Load("noShotFiredAchieved", false));

        TryUnlockIfTrue("SURGE_RELOAD",
            ES3.Load("surgeReloadAchieved", false));

        TryUnlockIfTrue("SPECIAL_WAVE",
            ES3.Load("specialWaveAchieved", false));

        TryUnlockIfTrue("EMBERLING_LATE",
            ES3.Load("emberlingLateAchieved", false));

        TryUnlockIfTrue("LOOSE_RESPAWNING",
            ES3.Load("loseRespawning", false));


        TryUnlockIfTrue("FUEL_FIRE_ALMOST_EMPTY",
            ES3.Load("fuelFireAlmostEmpty", false));

        TryUnlockIfTrue("VERDANT_GRAVEYARD",
           ES3.Load("verdantGraveyardAchieved", false));

        TryUnlockIfTrue("FIRST_DOG_ABILITY",
           ES3.Load("firstDogAbilityAchieved", false));

        TryUnlockIfTrue("ORB_ALCHEMIST",
           ES3.Load("orbAlchemistAchieved", false));


        // =========================
        // PROGRESSION / STATS
        // =========================

        ImportStatAndUnlock(
            statId: "KILLED_CREATURES",
            threshold: 5000,
            achievementId: "KILL_ENEMIES"
        );

        ImportStatAndUnlock(
            statId: "KILLED_CREATURES_INSIDE_FIRE_v3",
            threshold: 500,
            achievementId: "KILL_ENEMIES_IN_FIRE"
        );

        ImportStatAndUnlock(
            statId: "NIGHTS_SURVIVED_v3",
            threshold: 100,
            achievementId: "TOTAL_NIGHTS_SURVIVED"
        );

        ImportStatAndUnlock(
            statId: "RESOURCES_DUG_BY_DOG_v3",
            threshold: 50,
            achievementId: "DOG_RESOURCES"
        );

        ImportStatAndUnlock(
            statId: "RESOURCES_DROPPED_BY_DOG",
            threshold: 50,
            achievementId: "DOG_FETCH"
        );

        ImportStatAndUnlock(
            statId: "CREATURES_KILLED_BY_DOG",
            threshold: 50,
            achievementId: "DOG_KILL_CREATURES"
        );

        ImportStatAndUnlock(
            statId: "SECONDARY_FIRES_LIT_v3",
            threshold: 4,
            achievementId: "SECONDARY_FIRES"
        );


        // =========================
        // UNLOCKS / BOOLS
        // =========================

        TryUnlockIfTrue(
            "ALL_NPC_UNLOCKED",
            ES3.Load("mushroomMerchantUnlocked_ACHIEVEMENT", false) &&
            ES3.Load("architectTableUnlocked_ACHIEVEMENT", false)
        );

        TryUnlockIfTrue(
            "ALL_FIRE_UPGRADES",
            ES3.Load("fireFuelDepletionUnlocked_ACHIEVEMENT", false) &&
            ES3.Load("fireOrbConversionRate_ACHIEVEMENT", false) &&
            ES3.Load("fireFuelCapacity_ACHIEVEMENT", false)
        );

        SteamUserStats.StoreStats();

        Debug.Log("[Achievements] Import terminé");
    }

    private void ImportStatAndUnlock(string statId, int threshold, string achievementId) {
        int value = ES3.Load(statId, 0);
        if (value <= 0) return;

        SteamUserStats.SetStat(statId, value);

        if (value >= threshold && !IsThisAchievementUnlocked(achievementId)) {
            UnlockAchievement(achievementId);
        }
    }

    private void TryUnlockIfTrue(string achievementId, bool condition) {
        if (!condition) return;
        if (IsThisAchievementUnlocked(achievementId)) return;

        UnlockAchievement(achievementId);
    }

    [Button]
    public bool IsThisAchievementUnlocked(string id) {
        if (!playerConnected) return false;

        var ach = new Steamworks.Data.Achievement(id);
        return ach.State;
    }

    [Button]
    public void UnlockAchievement(string id) {
        if (!playerConnected) return;

        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();
        SteamUserStats.StoreStats();
    }

    [Button]
    public void ClearAchievementStatus(string id) {
        if (!playerConnected) return;

        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();
        SteamUserStats.StoreStats();
    }



    [Button]
    public int GetSteamStat(string id) {
        if (!playerConnected) return 0;
        return SteamUserStats.GetStatInt(id);
    }

    [Button]
    public void SetSteamStat(string id, int value) {
        if (!playerConnected) return;

        SteamUserStats.SetStat(id, value);
        SteamUserStats.StoreStats();
    }

    [Button]
    public void AddToSteamStat(string id, int value) {
        if (!playerConnected) return;

        SteamUserStats.AddStat(id, value);
        SteamUserStats.StoreStats();
    }

    [Button]
    public void SaveSteamStats() {
        if (!playerConnected) return;
        SteamUserStats.StoreStats();
    }
}
