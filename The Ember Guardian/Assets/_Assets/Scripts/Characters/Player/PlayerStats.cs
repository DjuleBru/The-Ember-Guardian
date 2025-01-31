using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    [SerializeField] private PlayerStatsSO playerStatsSO;

    #region MOVEMENT
    private float initialMoveSpeed;
    private float initialMaxStamina;
    private float initialExhaustionTime;
    private float initialRunAccelerationFactor;
    private float initialAimingSightDecelerationFactor;
    private float initialCrouchDetectionRangeReductionFactor;

    private float moveSpeed;
    private float maxStamina;
    private float exhaustionTime;
    private float runAccelerationFactor;
    private float runAccelerationFactorBuff;
    private float aimingSightDecelerationFactor;

    private float runAccelerationFactorBuff_meta;
    private float maxStaminaBuff_meta;
    private float moveSpeedPercentBuff_meta;
    private float runStaminaDepletionPercentBuff_meta;
    private float crouchDetectionRangeReductionPercentBuff_meta;
    private float rollForcePercentBuff_meta;
    private float rollStaminaDepletionPercentBuff_meta;

    #endregion

    # region Health
    private int initialMaxPlayerHP = 3;
    private float initialDamagedImmunityTime = 1.5f;
    private float initialRespawnTime = 5f;
    private int initialPlayerRespawnHP = 3;
    private float initialHpRegenTimer = 0f;
    private float absoluteHpRegenTimer_meta = 0f;

    private int maxPlayerHP;
    private int respawnPlayerHP;
    private float damagedImmunityTime;
    private float respawnTime;
    private float hpRegenTime;

    private int maxPlayerHPBuffAbsolute_meta;
    private int respawnPlayerHPBuffAbsolute_meta;

    public event EventHandler OnPlayerMaxHPChanged;
    public event EventHandler OnPlayerHPRegenChanged;

    #endregion

    #region SHOOTING
    private bool hold2WeaponsUnlocked;

    private float shootCooldownTime;
    private float reloadTime;
    private float handsReloadTime;

    private float swapWeaponTimeReductionPercentBuff_meta;
    private float flashlightRange;
    private float initialSwapWeaponTimeReductionPercent = 0;
    private float initialflashlightRange;
    private float flashlightRangeBuff_meta;
    #endregion

    #region BACKPACK

    [SerializeField] private int initialStartLevelAmmo;
    [SerializeField] private int initialStartLevelOrbs;
    [SerializeField] private float backpackGemSizePercentBuff;
    [SerializeField] private float backpackOrbSizePercentBuff;
    [SerializeField] private float backpackAmmoSizePercentBuff;

    private int startLevelAmmo;
    private int startLevelOrbs;
    private int startLevelAmmo_BuffAbsolute;
    private int startLevelOrbs_BuffAbsolute;
    #endregion

    #region OTHER
    private float shieldRegenTime;
    private float ammoRegenTime;
    private float chanceToDropx2;

    public event EventHandler OnPlayerShieldRegenTimeChanged;
    public event EventHandler OnPlayerAmmoRegenTimeChanged;
    public event EventHandler OnMoveSpeedChanged;
    #endregion

    public event EventHandler OnFlashlightRangeChanged;
    public event EventHandler OnCanHold2WeaponsUnlocked;

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }

    private void InitializeParameters() {
        LoadPlayerStatsSO();
        LoadPlayerStatBuffs_Meta();
        SetTempPlayerStatBuffs();
        RefreshCurrentPlayerStats();
    }

    private void LoadPlayerStatsSO() {

        initialMoveSpeed = playerStatsSO.initialMoveSpeed;
        initialMaxStamina = playerStatsSO.initialMaxStamina;
        initialExhaustionTime = playerStatsSO.initialExhaustionTime;
        initialRunAccelerationFactor = playerStatsSO.initialRunAccelerationFactor;
        initialAimingSightDecelerationFactor = playerStatsSO.initialAimingSightDecelerationFactor;
        initialflashlightRange = playerStatsSO.flashlightRange;

        initialStartLevelAmmo = playerStatsSO.initialStartLevelAmmo;
        startLevelOrbs = playerStatsSO.initialStartLevelOrbs;

        initialMaxPlayerHP = playerStatsSO.initialMaxPlayerHP;
        initialPlayerRespawnHP = playerStatsSO.initialPlayerRespawnHP;
        initialDamagedImmunityTime = playerStatsSO.initialDamagedImmunityTime;
        initialRespawnTime = playerStatsSO.initialRespawnTime;
        initialHpRegenTimer = playerStatsSO.initialHpRegenTimer;

    }

    private void LoadPlayerStatBuffs_Meta() {
        exhaustionTime = ES3.Load("exhaustionTime", 0);

        maxStaminaBuff_meta = ES3.Load("maxStaminaBuff_meta", 0f);
        moveSpeedPercentBuff_meta = ES3.Load("moveSpeedPercentBuff_meta", 0f);
        swapWeaponTimeReductionPercentBuff_meta = ES3.Load("swapWeaponTimeReductionPercentBuff_meta", 0f);
        crouchDetectionRangeReductionPercentBuff_meta = ES3.Load("crouchDetectionRangeReductionPercentBuff_meta", 0f);
        runStaminaDepletionPercentBuff_meta = ES3.Load("runStaminaDepletionPercentBuff_meta", 0f);
        rollForcePercentBuff_meta = ES3.Load("rollForcePercentBuff_meta", 0f);
        rollStaminaDepletionPercentBuff_meta = ES3.Load("rollStaminaDepletionPercentBuff_meta", 0f);
        flashlightRangeBuff_meta = ES3.Load("flashlightRangeBuff_meta", 0f);
        absoluteHpRegenTimer_meta = ES3.Load("absoluteHpRegenTimer_meta", 0f);
        maxPlayerHPBuffAbsolute_meta = ES3.Load("maxPlayerHPBuffAbsolute_meta", 0);
        respawnPlayerHPBuffAbsolute_meta = ES3.Load("respawnPlayerHPBuffAbsolute_meta", 0);

        startLevelAmmo_BuffAbsolute = ES3.Load("startLevelAmmo_BuffAbsolute", 0);
        startLevelOrbs_BuffAbsolute = ES3.Load("startLevelOrbs_BuffAbsolute", 0);

        backpackGemSizePercentBuff = ES3.Load("backpackGemSizePercentBuff", 0f);
        backpackAmmoSizePercentBuff = ES3.Load("backpackAmmoSizePercentBuff", 0f);
        backpackOrbSizePercentBuff = ES3.Load("backpackOrbSizePercentBuff", 0f);

        hold2WeaponsUnlocked = ES3.Load("hold2WeaponsUnlocked", false);
    }

    private void SetTempPlayerStatBuffs() {
        runAccelerationFactorBuff = runAccelerationFactorBuff_meta;
    }

    private void RefreshCurrentPlayerStats() {
        moveSpeed = initialMoveSpeed + initialMoveSpeed * moveSpeedPercentBuff_meta / 100;
        maxStamina = initialMaxStamina + initialMaxStamina * maxStaminaBuff_meta / 100;
        exhaustionTime = initialExhaustionTime - initialExhaustionTime * exhaustionTime / 100;
        runAccelerationFactor = initialRunAccelerationFactor + initialRunAccelerationFactor * runAccelerationFactorBuff_meta / 100;
        flashlightRange = initialflashlightRange + flashlightRangeBuff_meta;

        startLevelAmmo = initialStartLevelAmmo + startLevelAmmo_BuffAbsolute;
        startLevelOrbs = initialStartLevelOrbs + startLevelOrbs_BuffAbsolute;

        maxPlayerHP = initialMaxPlayerHP + maxPlayerHPBuffAbsolute_meta;
        respawnPlayerHP = initialPlayerRespawnHP + respawnPlayerHPBuffAbsolute_meta;
        hpRegenTime = initialHpRegenTimer + absoluteHpRegenTimer_meta;

        damagedImmunityTime = initialDamagedImmunityTime;
        aimingSightDecelerationFactor = initialAimingSightDecelerationFactor;
        respawnTime = initialRespawnTime;
    }


    #region SET PLAYER PERMANENT BUFFS

    public void UnlockCanHold2Weapons() {
        hold2WeaponsUnlocked = true;
        OnCanHold2WeaponsUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public void SetMoveSpeedBuff(float moveSpeedBuff) {
        this.moveSpeedPercentBuff_meta = moveSpeedBuff;
        RefreshCurrentPlayerStats();

        OnMoveSpeedChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetMaxStaminaBuff(float maxStaminaBuff) {
        this.maxStaminaBuff_meta = maxStaminaBuff;
        RefreshCurrentPlayerStats();
    }

    public void SetRunAccelerationFactorBuff(float runAccelerationFactorBuff) {
        this.runAccelerationFactorBuff_meta = runAccelerationFactorBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetRunStaminaCostBuff(float runStaminaCost) {
        this.runStaminaDepletionPercentBuff_meta = runStaminaCost;
        RefreshCurrentPlayerStats();
    }

    public void SetCrouchDetectionRangeBuff(float crouchDetectionRangeBuff) {
        this.crouchDetectionRangeReductionPercentBuff_meta = crouchDetectionRangeBuff;
        RefreshCurrentPlayerStats();
    }

    public void SetRollForceBuff(float rollForcePercentBuff) {
        this.rollForcePercentBuff_meta = rollForcePercentBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetRollStaminaCostBuff(float rollStaminaCostBuff) {
        this.rollStaminaDepletionPercentBuff_meta = rollStaminaCostBuff;
        RefreshCurrentPlayerStats();
    }

    public void SetMaxPlayerHPBuff(int maxPlayerHPBuffAbsolute) {
        this.maxPlayerHPBuffAbsolute_meta = maxPlayerHPBuffAbsolute;
        RefreshCurrentPlayerStats();
    }

    public void SetRespawnPlayerHPBuff(int respawnPlayerHPBuffAbsolute) {
        this.respawnPlayerHPBuffAbsolute_meta = respawnPlayerHPBuffAbsolute;
        RefreshCurrentPlayerStats();
    }

    public void SetSwapWeaponTimeReductionPercent(float swapWeaponTimeReductionPercent) {
        this.swapWeaponTimeReductionPercentBuff_meta = swapWeaponTimeReductionPercent;
        RefreshCurrentPlayerStats();
    }

    public void SetFlashlightRangeBuff(int flashlightRangeBuff) {
        this.flashlightRangeBuff_meta = flashlightRangeBuff;
        RefreshCurrentPlayerStats();
        OnFlashlightRangeChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SetInitialStartLevelAmmoBuff(int initialStartLevelAmmoBuff) {
        this.startLevelAmmo_BuffAbsolute = initialStartLevelAmmoBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetInitialStartLevelOrbsBuff(int initialStartLevelOrbsBuff) {
        this.startLevelOrbs_BuffAbsolute = initialStartLevelOrbsBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetBackpackOrbSizeBuff(float orbSizeBuff) {
        this.backpackOrbSizePercentBuff = orbSizeBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetBackpackGemSizeBuff(float gemSizeBuff) {
        this.backpackGemSizePercentBuff = gemSizeBuff;
        RefreshCurrentPlayerStats();
    }
    public void SetBackpackAmmoSizeBuff(float ammoSizeBuff) {
        this.backpackAmmoSizePercentBuff = ammoSizeBuff;
        RefreshCurrentPlayerStats();
    }

    public void SetHpRegenTimeAbsolute(float hpRegenTime) {
        absoluteHpRegenTimer_meta = hpRegenTime;
        RefreshCurrentPlayerStats();

        OnPlayerHPRegenChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region GET PARAMETERS

    public bool GetHold2WeaponsUnlocked() {
        return hold2WeaponsUnlocked;
    }

    public int GetMaxHP() {
        return maxPlayerHP;
    }

    public int GetPlayerRespawnHP() {
        return respawnPlayerHP;
    }
    public int GetStartLevelAmmo() {
        return startLevelAmmo;
    }
    public int GetStartLevelOrbs() {
        return startLevelOrbs;
    }
    public float GetDamagedImmunityTime() {
        return damagedImmunityTime;
    }

    public float GetRespawnTime() {
        return respawnTime;
    }

    public float GetHpRegenTime() {
        return hpRegenTime;
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public float GetMaxStamina() {
        return maxStamina;
    }

    public float GetExhaustionTime() {
        return exhaustionTime;
    }

    public float GetRunAccelerationFactor() {
        return runAccelerationFactor;
    }

    public float GetAimingSightDecelerationFactor() {
        return aimingSightDecelerationFactor;
    }

    public float GetAmmoRegenTime() {
        return ammoRegenTime;
    }

    public float GetChanceToDropx2() {
        return chanceToDropx2;
    }

    public float GetReloadTime() {
        return reloadTime;
    }

    public float GetHandsReloadTime() {
        return handsReloadTime;
    }

    public float GetShootCooldownTime() {
        return shootCooldownTime;
    }

    public float GetFlashlightRange() {
        return flashlightRange;
    }

    #endregion

    #region GET INITIAL PARAMETERS

    public int GetInitialPlayerMaxHP() {
        return initialMaxPlayerHP;
    }
    public int GetInitialPlayerRespawnHP() {
        return initialPlayerRespawnHP;
    }
    public int GetInitialStartLevelAmmo() {
        return initialStartLevelAmmo;
    }
    public int GetInitialStartLevelOrbs() {
        return initialStartLevelOrbs;
    }

    public float GetInitialDamagedImmunityTime() {
        return initialDamagedImmunityTime;
    }

    public float GetInitialRespawnTime() {
        return initialRespawnTime;
    }

    public float GetInitialHpRegenTimer() {
        return initialHpRegenTimer;
    }

    public float GetInitialMoveSpeed() {
        return initialMoveSpeed;
    }

    public float GetInitialMaxStamina() {
        return initialMaxStamina;
    }

    public float GetInitialExhaustionTime() {
        return initialExhaustionTime;
    }

    public float GetInitialRunAccelerationFactor() {
        return initialRunAccelerationFactor;
    }
    public float GetInitialSwapWeaponTimeReductionPercent() {
        return initialSwapWeaponTimeReductionPercent;
    }
    public float GetInitialFlashlightRange() {
        return initialflashlightRange;
    }
    public float GetInitialCrouchDetectionRangeReductionFactor() {
        return initialCrouchDetectionRangeReductionFactor;
    }
    #endregion

    #region GET PARAMETER BUFFS META

    public bool GetCanHold2WeaponsUnlocked() {
        return hold2WeaponsUnlocked;
    }
    public float GetRunAccelerationFactorBuff_Meta() {
        return runAccelerationFactorBuff_meta;
    }

    public float GetSwapWeaponTimeReductionPercentBuff_Meta() {
        return swapWeaponTimeReductionPercentBuff_meta;
    }

    public float GetCrouchDetectionRangeReductionPercentBuff_Meta() {
        return crouchDetectionRangeReductionPercentBuff_meta;
    }

    public float GetRunStaminaDepletionPercentBuff_Meta() {
        return runStaminaDepletionPercentBuff_meta;
    }
    public float GetMoveSpeedPercentBuff_Meta() {
        return moveSpeedPercentBuff_meta;
    }

    public float GetMaxStaminaPercentBuff_Meta() {
        return maxStaminaBuff_meta;
    }
    public int GetStartLevelAmmoBuff_Meta() {
        return startLevelAmmo_BuffAbsolute;
    }
    public int GetStartLevelOrbsBuff_Meta() {
        return startLevelOrbs_BuffAbsolute;
    }

    public float GetRollForcePercentBuff_Meta() {
        return rollForcePercentBuff_meta;
    }
    public float GetRollStaminaDepletionPercentBuff_Meta() {
        return rollStaminaDepletionPercentBuff_meta;
    }
    public float GetBackpackGemSizePercentBuff_Meta() {
        return backpackGemSizePercentBuff;
    }
    public float GetBackpackAmmoSizePercentBuff_Meta() {
        return backpackAmmoSizePercentBuff;
    }
    public float GetBackpackOrbSizePercentBuff_Meta() {
        return backpackOrbSizePercentBuff;
    }
    #endregion

    #region SET SHOOT PARAMETERS
    public void SetShootCooldownTime(float shootCooldownTime) {
        this.shootCooldownTime = shootCooldownTime;
    }

    public void SetReloadTime(float reloadTime) {
        this.reloadTime = reloadTime;
    }

    public void SetHandsReloadTime(float handsReloadTime) {
        this.handsReloadTime = handsReloadTime;
    }
    #endregion

    #region BUFF TEMP PARAMETERS

    public void BuffMoveSpeed(float buffAmount) {
        Debug.Log("BuffMoveSpeed " + initialMoveSpeed * buffAmount);
        moveSpeed += initialMoveSpeed * buffAmount;
        OnMoveSpeedChanged?.Invoke(this, EventArgs.Empty);
    }
    public void DebuffMoveSpeed(float buffAmount) {
        Debug.Log("DebuffMoveSpeed " + initialMoveSpeed * buffAmount);
        moveSpeed -= initialMoveSpeed * buffAmount;
        OnMoveSpeedChanged?.Invoke(this, EventArgs.Empty);
    }

    public void BuffShootCooldown(float buffAmount) {
        //Debug.Log("BuffShootCooldown " + buffAmount);
        shootCooldownTime /= buffAmount;
    }

    public void DebuffShootCooldown(float buffAmount) {
        //Debug.Log("DebuffShootCooldown " +buffAmount);
        shootCooldownTime *= buffAmount;
    }

    public void BuffRunAccelerationFactor(float buffAmount) {
        Debug.Log("BuffRunAccelerationFactor " + (initialRunAccelerationFactor-1) * buffAmount);
        runAccelerationFactorBuff += (initialRunAccelerationFactor - 1) *  buffAmount;
    }

    public void BuffStamina(float buffAmount) {
        Debug.Log("BuffStamina " + buffAmount);
        maxStamina += buffAmount;
    }

    public void BuffMaxHP(int buffAmount) {
        Debug.Log("BuffMaxHP " + buffAmount);
        maxPlayerHP += buffAmount;
        OnPlayerMaxHPChanged?.Invoke(this, EventArgs.Empty);
    }

    public void BuffPlayerHealthRegen(float buffAmount) {
        Debug.Log("BuffPlayerHealthRegen " + buffAmount);
        hpRegenTime = buffAmount;
        OnPlayerHPRegenChanged?.Invoke(this, EventArgs.Empty);
    }

    public void BuffPlayerAmmoRegen(float buffAmount) {
        Debug.Log("BuffPlayerAmmoRegen " + buffAmount);
        ammoRegenTime = buffAmount;
        OnPlayerAmmoRegenTimeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void BuffChanceToDropx2(float buffAmount) {
        Debug.Log("BuffChanceToDropx2 " + buffAmount);
        chanceToDropx2 += buffAmount;
    }
    #endregion

    public void SaveMetaBuffValues() {
        ES3.Save("swapWeaponTimeReductionPercentBuff_meta", swapWeaponTimeReductionPercentBuff_meta);

        ES3.Save("maxStaminaBuff_meta", maxStaminaBuff_meta);
        ES3.Save("moveSpeedPercentBuff_meta", moveSpeedPercentBuff_meta);

        ES3.Save("crouchDetectionRangeReductionPercentBuff_meta", crouchDetectionRangeReductionPercentBuff_meta);
        ES3.Save("runStaminaDepletionPercentBuff_meta", runStaminaDepletionPercentBuff_meta);
        ES3.Save("rollForcePercentBuff_meta", rollForcePercentBuff_meta);
        ES3.Save("rollStaminaDepletionPercentBuff_meta", rollStaminaDepletionPercentBuff_meta);

        ES3.Save("maxPlayerHPBuffAbsolute_meta", maxPlayerHPBuffAbsolute_meta);
        ES3.Save("respawnPlayerHPBuffAbsolute_meta", respawnPlayerHPBuffAbsolute_meta);
        ES3.Save("absoluteHpRegenTimer_meta", absoluteHpRegenTimer_meta);

        ES3.Save("startLevelAmmo_BuffAbsolute", startLevelAmmo_BuffAbsolute);
        ES3.Save("startLevelOrbs_BuffAbsolute", startLevelOrbs_BuffAbsolute);
        ES3.Save("flashlightRangeBuff_meta", flashlightRangeBuff_meta);

        ES3.Save("backpackGemSizePercentBuff", backpackGemSizePercentBuff);
        ES3.Save("backpackAmmoSizePercentBuff", backpackAmmoSizePercentBuff);
        ES3.Save("backpackOrbSizePercentBuff", backpackOrbSizePercentBuff);

        ES3.Save("hold2WeaponsUnlocked", hold2WeaponsUnlocked);



        Debug.Log("SaveMetaBuffValues flashlightRangeBuff_meta" + startLevelOrbs_BuffAbsolute);
    }

    public float GetSkillStat(SkillItem skillItem) {
        float skillStat = 0f;

        switch (skillItem.skillType) {

            case SkillItem.SkillType.passiveMaxHPIncrease:
                skillStat = maxPlayerHP;
                break;

            case SkillItem.SkillType.passiveShieldGenerator:
                skillStat = shieldRegenTime;
                break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                skillStat = runAccelerationFactorBuff_meta;
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                skillStat = hpRegenTime;
                break;
            case SkillItem.SkillType.passiveAmmoGenerator:
                skillStat = ammoRegenTime;
                break;
            case SkillItem.SkillType.passiveMoveSpeedBuff:
                skillStat = moveSpeed;
                break;
        }

        return skillStat;
    }
}
