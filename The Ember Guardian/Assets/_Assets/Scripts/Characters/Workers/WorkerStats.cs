using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStats : MonoBehaviour
{
    public static WorkerStats Instance;

    private bool interactionWithWorkersUnlocked;
    private bool workerInteractions_Debug;
    private int initialMaxFollowingWorkers = 3;
    private int maxFollowingWorkers;
    private int initialEmberlings;
    private int emberlingArrivalsNumber;

    private float headToCampMoveSpeed = 2.5f;
    private float roamMoveSpeed = 1.5f;
    private float fleeMoveSpeed = 3.5f;

    private int initialHunterHealth = 2;
    private int initialHunterDamage = 10;
    private float initialHunterAttackCooldown = 3f;
    private float initialHunterAccuracy = 1.5f;
    private int hunterDamageBuff;
    private int hunterHealthBuff;
    private float hunterAttackCooldownBuff;
    private float hunterAccuracyBuff;
    private float hunterMoveSpeedBuff;

    private int initialGuardDamage = 5;
    private float initialGuardAttackCooldown = 1f;
    private float initialGuardHealth = 20f;
    private int guardDamageBuff;
    private float guardAttackCooldownBuff;
    private int guardHealthBuff;
    private float guardMoveSpeedBuff;

    private float engineerMoveSpeedBuff;
    private float engineerWrenchSpeedBuff;

    private int initialMinerHealth = 1;
    private int initialMinerDamage = 1;
    private float initialMinerAttackCooldown = 1.2f;
    private float initialMinerLuckyPickaxeProb = 0f;
    private int minerDamageBuff;
    private float minerAttackCooldownBuff;
    private int minerHealthBuff;
    private float minerPickaxeLuckyProb;
    private float minerMoveSpeedBuff;

    public event EventHandler OnInteractionsWithWorkersUnlocked;
    
    private void Awake() {
        Instance = this;
        LoadStatValues();
    }

    private void LoadStatValues() {
        workerInteractions_Debug = DebugManager.Instance.GetDebugMode_WorkerInteractions();
        interactionWithWorkersUnlocked = ES3.Load("interactionWithWorkersUnlocked", workerInteractions_Debug);

        if (!ES3.KeyExists("WorkerStats"))
            return;

        var workerData = ES3.Load<Dictionary<string, object>>("WorkerStats");

        // Global
        maxFollowingWorkers = GetValue(workerData, "maxFollowingWorkers", initialMaxFollowingWorkers);
        initialEmberlings = GetValue(workerData, "initialEmberlings", 0);
        emberlingArrivalsNumber = GetValue(workerData, "emberlingArrivalsNumber", 0);

        // Hunter
        hunterHealthBuff = GetValue(workerData, "hunterHealthBuff", 0);
        hunterDamageBuff = GetValue(workerData, "hunterDamageBuff", 0);
        hunterAttackCooldownBuff = GetValue(workerData, "hunterAttackCooldownBuff", 0f);
        hunterMoveSpeedBuff = GetValue(workerData, "hunterMoveSpeedBuff", 0f);
        hunterAccuracyBuff = GetValue(workerData, "hunterAccuracyBuff", 0f);

        // Guard
        guardHealthBuff = GetValue(workerData, "guardHealthBuff", 0);
        guardDamageBuff = GetValue(workerData, "guardDamageBuff", 0);
        guardAttackCooldownBuff = GetValue(workerData, "guardAttackCooldownBuff", 0f);
        guardMoveSpeedBuff = GetValue(workerData, "guardMoveSpeedBuff", 0f);

        // Miner
        minerHealthBuff = GetValue(workerData, "minerHealthBuff", 0);
        minerDamageBuff = GetValue(workerData, "minerDamageBuff", 0);
        minerAttackCooldownBuff = GetValue(workerData, "minerAttackCooldownBuff", 0f);
        minerMoveSpeedBuff = GetValue(workerData, "minerMoveSpeedBuff", 0f);
        minerPickaxeLuckyProb = GetValue(workerData, "minerPickaxeLuckyProb", 0f);

        // Engineer
        engineerMoveSpeedBuff = GetValue(workerData, "engineerMoveSpeedBuff", 0f);
        engineerWrenchSpeedBuff = GetValue(workerData, "engineerWrenchSpeedBuff", 0f);
    }

    private T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue) {
        if (dict.ContainsKey(key)) {
            try {
                return (T)Convert.ChangeType(dict[key], typeof(T));
            }
            catch {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void SetInteractionWithWorkersUnlocked() {
        interactionWithWorkersUnlocked = true;

        ES3.Save("interactionWithWorkersUnlocked", true);
        OnInteractionsWithWorkersUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public void SetMaxFollowingWorkers(int maxFollowingWorkersBuff) {
        int maxFollowingWorkersTotal = initialMaxFollowingWorkers + maxFollowingWorkersBuff;
        maxFollowingWorkers = maxFollowingWorkersTotal;
    }
    public void SetInitialEmberlings(int initialEmberlings) {
        this.initialEmberlings = initialEmberlings;
    }
    public void SetEmberlingsArrivalsNumber(int emberlingArrivalsNumber) {
        Debug.Log("SetEmberlingsArrivalsNumber " + emberlingArrivalsNumber);
        this.emberlingArrivalsNumber = emberlingArrivalsNumber;
    }

    #region GET INITIAL VALUES
    public float GetRoamMoveSpeed() {
        return roamMoveSpeed;
    }
    public float GetHeadToCampMoveSpeed() {
        return headToCampMoveSpeed;
    }
    public float GetFleeMoveSpeed() {
        return fleeMoveSpeed;
    }
    public int GetInitialMaxFollowingWorkers() {
        return initialMaxFollowingWorkers;
    }
    public int GetInitialHunterDamage() {
        return initialHunterDamage;
    }
    public int GetInitialGuardDamage() {
        return initialGuardDamage;
    }
    public int GetInitialMinerDamage() {
        return initialMinerDamage;
    }
    public float GetInitialHunterAttackCooldown() {
        return initialHunterAttackCooldown;
    }
    public float GetInitialMinerAttackCooldown() {
        return initialMinerAttackCooldown;
    }
    public float GetInitialGuardAttackCooldown() {
        return initialGuardAttackCooldown;
    }
    public float GetInitialHunterHealth() {
        return initialHunterHealth;
    }
    public float GetInitialMinerHealth() {
        return initialMinerHealth;
    }
    public float GetInitialGuardHealth() {
        return initialGuardHealth;
    }
    public float GetInitialHunterAccuracy() {
        return initialHunterAccuracy;
    }
    public float GetInitialHunterAccuracyBuff() {
        return 0;
    }
    public float GetInitialMinerLuckyPickaxeProb() {
        return initialMinerLuckyPickaxeProb;
    }

    #endregion

    #region GET VALUES
    public bool GetInteractionWithWorkersUnlocked() {
        return interactionWithWorkersUnlocked;
    }
    public int GetMaxFollowingWorkers() {
        return maxFollowingWorkers;
    }
    public int GetInitialEmberlings() {
        return initialEmberlings;
    }
    public float GetEmberlingsArrivalsNumber() {
        return emberlingArrivalsNumber;
    }

    public int GetHunterDamage() {
        return initialHunterDamage + hunterDamageBuff;
    }
    public int GetGuardDamage() {
        return initialGuardDamage + guardDamageBuff;
    }
    public int GetMinerDamage() {
        return initialMinerDamage + minerDamageBuff;
    }
    public float GetHunterAttackCooldown() {
        return initialHunterAttackCooldown + hunterAttackCooldownBuff;
    }
    public float GetMinerAttackCooldown() {
        return initialMinerAttackCooldown + minerAttackCooldownBuff;
    }
    public float GetGuardAttackCooldown() {
        return initialGuardAttackCooldown + guardAttackCooldownBuff;
    }
    public float GetHunterHealth() {
        return initialHunterHealth + hunterHealthBuff;
    }
    public float GetMinerHealth() {
        return initialMinerHealth + minerHealthBuff;
    }
    public float GetGuardHealth() {
        return initialGuardHealth + guardHealthBuff;
    }
    public float GetHunterAccuracy() {
        return initialHunterAccuracy - hunterAccuracyBuff/100f;
    }
    public float GetHunterAccuracyBuff() {
        return hunterAccuracyBuff;
    }
    public float GetMinerLuckyPickaxeProb() {
        return initialMinerLuckyPickaxeProb + minerPickaxeLuckyProb;
    }
    public float GetHunterMoveSpeedBuff() {
        return hunterMoveSpeedBuff;
    }

    public float GetMinerMoveSpeedBuff() {
        return minerMoveSpeedBuff;
    }

    public float GetGuardMoveSpeedBuff() {
        return guardMoveSpeedBuff;
    }
    public float GetEngineerMoveSpeedBuff() {
        return engineerMoveSpeedBuff;
    }
    public float GetEngineerWrenchSpeedBuff() {
        return engineerWrenchSpeedBuff;
    }

    #endregion

    #region SET BUFF VALUES

    public void SetHunterHealthBuff(int hunterHealthBuff) {
        this.hunterHealthBuff = hunterHealthBuff;
    }
    public void SetGuardHealthBuff(int guardHealthBuff) {
        this.guardHealthBuff = guardHealthBuff;
    }
    public void SetMinerHealthBuff(int minerHealthBuff) {
        this.minerHealthBuff = minerHealthBuff;
    }
    public void SetHunterDamageBuff(int hunterDamageBuff) {
        this.hunterDamageBuff = hunterDamageBuff;
    }
    public void SetGuardDamageBuff(int guardDamageBuff) {
        this.guardDamageBuff = guardDamageBuff;
    }
    public void SetMinerDamageBuff(int minerDamageBuff) {
        this.minerDamageBuff = minerDamageBuff;
    }

    public void SetHunterAttackCooldownBuff(float hunterAttackCooldownBuff) {
        this.hunterAttackCooldownBuff = hunterAttackCooldownBuff;
    }
    public void SetGuardAttackCooldownBuff(float guardAttackCooldownBuff) {
        this.guardAttackCooldownBuff = guardAttackCooldownBuff;
    }
    public void SetMinerAttackCooldownBuff(float minerAttackCooldownBuff) {
        this.minerAttackCooldownBuff = minerAttackCooldownBuff;
    }

    public void SetHunterMoveSpeedBuff(float hunterMoveSpeedBuff) {
        this.hunterMoveSpeedBuff = hunterMoveSpeedBuff;
    }
    public void SetMinerMoveSpeedBuff(float minerMoveSpeedBuff) {
        this.minerMoveSpeedBuff = minerMoveSpeedBuff;
    }
    public void SetGuardMoveSpeedBuff(float guardMoveSpeedBuff) {
        this.guardMoveSpeedBuff = guardMoveSpeedBuff;
    }
    public void SetEngineerMoveSpeedBuff(float engineerMoveSpeedBuff) {
        this.engineerMoveSpeedBuff = engineerMoveSpeedBuff;
    }
    public void SetEngineerWrenchSpeedBuff(float engineerWrenchSpeedBuff) {
        this.engineerWrenchSpeedBuff = engineerWrenchSpeedBuff;
    }
    public void SetHunterAccuracyBuff(float hunterAccuracyBuff) {
        this.hunterAccuracyBuff = hunterAccuracyBuff;
    }
    public void SetMinerLuckyPickaxeProb(float minerPickaxeLuckyProb) {
        this.minerPickaxeLuckyProb = minerPickaxeLuckyProb;
    }


    #endregion

    #region UNLOCK SHRINES

    [Button]
    public void UnlockShrineType(StructureSO.StructureType structureType) {
        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(structureType.ToString() + "1", true);
    }
    #endregion

    public void SaveWorkerValues() {
        var workerData = new Dictionary<string, object>();

        // Global
        workerData["maxFollowingWorkers"] = maxFollowingWorkers;
        workerData["initialEmberlings"] = initialEmberlings;
        workerData["emberlingArrivalsNumber"] = emberlingArrivalsNumber;

        // Hunter
        workerData["hunterHealthBuff"] = hunterHealthBuff;
        workerData["hunterDamageBuff"] = hunterDamageBuff;
        workerData["hunterAttackCooldownBuff"] = hunterAttackCooldownBuff;
        workerData["hunterMoveSpeedBuff"] = hunterMoveSpeedBuff;
        workerData["hunterAccuracyBuff"] = hunterAccuracyBuff;

        // Guard
        workerData["guardHealthBuff"] = guardHealthBuff;
        workerData["guardDamageBuff"] = guardDamageBuff;
        workerData["guardAttackCooldownBuff"] = guardAttackCooldownBuff;
        workerData["guardMoveSpeedBuff"] = guardMoveSpeedBuff;

        // Miner
        workerData["minerHealthBuff"] = minerHealthBuff;
        workerData["minerDamageBuff"] = minerDamageBuff;
        workerData["minerAttackCooldownBuff"] = minerAttackCooldownBuff;
        workerData["minerMoveSpeedBuff"] = minerMoveSpeedBuff;
        workerData["minerPickaxeLuckyProb"] = minerPickaxeLuckyProb;

        // Engineer
        workerData["engineerMoveSpeedBuff"] = engineerMoveSpeedBuff;
        workerData["engineerWrenchSpeedBuff"] = engineerWrenchSpeedBuff;

        ES3.Save("WorkerStats", workerData);
    }
}
