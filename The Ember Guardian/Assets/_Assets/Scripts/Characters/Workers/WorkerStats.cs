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
    private float initialGuardHealth = 10f;
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
        maxFollowingWorkers = ES3.Load("maxFollowingWorkers", initialMaxFollowingWorkers);
        initialEmberlings = ES3.Load("initialEmberlings", 0);
        emberlingArrivalsNumber = ES3.Load("emberlingArrivalsNumber", 0);

        hunterDamageBuff = ES3.Load("hunterDamageBuff", 0);
        hunterHealthBuff = ES3.Load("hunterHealthBuff", 0);
        hunterAttackCooldownBuff = ES3.Load("hunterAttackCooldownBuff", 0f);
        hunterAccuracyBuff = ES3.Load("hunterAccuracyBuff", 0f);
        hunterMoveSpeedBuff = ES3.Load("hunterMoveSpeedBuff", 0f);

        guardDamageBuff = ES3.Load("guardDamageBuff", 0);
        guardAttackCooldownBuff = ES3.Load("guardAttackCooldownBuff", 0f);
        guardHealthBuff = ES3.Load("guardHealthBuff", 0);
        guardMoveSpeedBuff = ES3.Load("guardMoveSpeedBuff", 0f);

        minerAttackCooldownBuff = ES3.Load("minerAttackCooldownBuff", 0f);
        minerDamageBuff = ES3.Load("minerDamageBuff", 0);
        minerHealthBuff = ES3.Load("minerHealthBuff", 0);
        minerPickaxeLuckyProb = ES3.Load("minerPickaxeLuckyProb", 0f);
        minerMoveSpeedBuff = ES3.Load("minerMoveSpeedBuff", 0f);

        engineerMoveSpeedBuff = ES3.Load("engineerMoveSpeedBuff", 0f);
        engineerWrenchSpeedBuff = ES3.Load("engineerWrenchSpeedBuff", 0f);
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
        ES3.Save("maxFollowingWorkers", maxFollowingWorkers);
        ES3.Save("initialEmberlings", initialEmberlings);
        ES3.Save("emberlingArrivalsNumber", emberlingArrivalsNumber);

        ES3.Save("hunterHealthBuff", hunterHealthBuff);
        ES3.Save("minerHealthBuff", minerHealthBuff);
        ES3.Save("guardHealthBuff", guardHealthBuff);
        ES3.Save("hunterDamageBuff", hunterDamageBuff);
        ES3.Save("guardDamageBuff", guardDamageBuff);
        ES3.Save("minerDamageBuff", minerDamageBuff);
        ES3.Save("hunterAttackCooldownBuff", hunterAttackCooldownBuff);
        ES3.Save("guardAttackCooldownBuff", guardAttackCooldownBuff);
        ES3.Save("minerAttackCooldownBuff", minerAttackCooldownBuff);
        ES3.Save("hunterMoveSpeedBuff", hunterMoveSpeedBuff);
        ES3.Save("minerMoveSpeedBuff", minerMoveSpeedBuff);
        ES3.Save("guardMoveSpeedBuff", guardMoveSpeedBuff);
        ES3.Save("engineerMoveSpeedBuff", engineerMoveSpeedBuff);
        ES3.Save("engineerWrenchSpeedBuff", engineerWrenchSpeedBuff);
        ES3.Save("hunterAccuracyBuff", hunterAccuracyBuff);
        ES3.Save("minerPickaxeLuckyProb", minerPickaxeLuckyProb);
    }
}
