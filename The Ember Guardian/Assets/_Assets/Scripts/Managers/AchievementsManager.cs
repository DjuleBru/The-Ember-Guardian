using Sirenix.OdinInspector;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AchievementsManager : MonoBehaviour
{

    public static AchievementsManager Instance;

    private void Awake() {
        Instance = this;
    }
 
    private void Update() {
        SteamClient.RunCallbacks();
    }

    [Button]
    public bool IsThisAchievementUnlocked(string id) {
        var ach = new Steamworks.Data.Achievement(id);
        Debug.Log("Achievement " + id + "status : " + ach.State);

        return ach.State;
    }

    [Button]
    public void UnlockAchievement(string id) {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();

        Debug.Log("Achievement " + id + "unlocked");
    }

    [Button]
    public void ClearAchievementStatus(string id) {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();

        Debug.Log("Achievement " + id + "Cleared : ");
    }

    [Button]
    public int GetSteamStat(string id) {
        Debug.Log(id + " = " + Steamworks.SteamUserStats.GetStatInt(id));
        return Steamworks.SteamUserStats.GetStatInt(id);
    }

    [Button]
    public void SetSteamStat(string id, int value) {
        Steamworks.SteamUserStats.SetStat(id, value);

        Debug.Log("Setting " + id + "to: " + value);
    }

    [Button]
    public void AddToSteamStat(string id, int value) {
        Steamworks.SteamUserStats.AddStat(id, value);

        Debug.Log("Adding " + value + " to: " + id);
    }

    [Button]
    public void SaveSteamStats() {
        Steamworks.SteamUserStats.StoreStats();
    }

}
