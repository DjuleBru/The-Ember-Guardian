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
