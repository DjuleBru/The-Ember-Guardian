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
    private float initialRunMaxTime;
    private float initialExhaustionTime;
    private float initialRunAccelerationFactor;
    private float initialAimingSightDecelerationFactor;

    private float moveSpeed;
    private float runMaxTime;
    private float exhaustionTime;
    private float runAccelerationFactor;
    private float aimingSightDecelerationFactor;

    #endregion

    # region Health
    private int initialMaxPlayerHP = 3;
    private float initialDamagedImmunityTime = 1.5f;
    private float initialRespawnTime = 5f;
    private int initialPlayerRespawnHealth = 3;
    private float initialHpRegenTimer = 0f;

    private int maxPlayerHP;
    private float damagedImmunityTime;
    private float respawnTime;
    private int playerRespawnHealth;
    private bool hasHpRegenPassive;
    private float hpRegenTime;

    public event EventHandler OnPlayerMaxHPChanged;
    public event EventHandler OnPlayerHPRegenChanged;

    #endregion

    #region SHOOTING
    private float shootCooldownTime;
    private float reloadTime;
    private float handsReloadTime;
    #endregion

    #region OTHER
    private float shieldRegenTime;
    private float ammoRegenTime;
    private float chanceToDropx2;

    public event EventHandler OnPlayerShieldRegenTimeChanged;
    public event EventHandler OnPlayerAmmoRegenTimeChanged;
    public event EventHandler OnMoveSpeedChanged;
    #endregion

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }

    private void InitializeParameters() {
        // LATER ADD IF VALUE SAVED THEN LOAD VALUE instead of playerStatsSO
        initialMoveSpeed = playerStatsSO.initialMoveSpeed;
        initialRunMaxTime = playerStatsSO.initialRunMaxTime;
        initialExhaustionTime = playerStatsSO.initialExhaustionTime;
        initialRunAccelerationFactor = playerStatsSO.initialRunAccelerationFactor;
        initialAimingSightDecelerationFactor = playerStatsSO.initialAimingSightDecelerationFactor;

        initialMaxPlayerHP = playerStatsSO.initialMaxPlayerHP;
        initialDamagedImmunityTime += playerStatsSO.initialDamagedImmunityTime;
        initialRespawnTime = playerStatsSO.initialRespawnTime;
        initialPlayerRespawnHealth = playerStatsSO.initialPlayerRespawnHealth;
        initialHpRegenTimer += playerStatsSO.initialHpRegenTimer;

        moveSpeed = playerStatsSO.initialMoveSpeed;
        runMaxTime = playerStatsSO.initialRunMaxTime;
        exhaustionTime = playerStatsSO.initialExhaustionTime;
        runAccelerationFactor = playerStatsSO.initialRunAccelerationFactor;
        aimingSightDecelerationFactor = playerStatsSO.initialAimingSightDecelerationFactor;

        maxPlayerHP = playerStatsSO.initialMaxPlayerHP;
        damagedImmunityTime += playerStatsSO.initialDamagedImmunityTime;
        respawnTime = playerStatsSO.initialRespawnTime;
        playerRespawnHealth = playerStatsSO.initialPlayerRespawnHealth;
        hpRegenTime += playerStatsSO.initialHpRegenTimer;
    }

    #region GET PARAMETERS

    public int GetPlayerMaxHP() {
        return maxPlayerHP;
    }

    public float GetDamagedImmunityTime() {
        return damagedImmunityTime;
    }

    public float GetRespawnTime() {
        return respawnTime;
    }

    public int GetPlayerRespawnHealth() {
        return playerRespawnHealth;
    }

    public float GetHpRegenTime() {
        return hpRegenTime;
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public float GetRunMaxTime() {
        return runMaxTime;
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

    #endregion

    #region GET INITIAL PARAMETERS

    public int GetInitialPlayerMaxHP() {
        return initialMaxPlayerHP;
    }

    public float GetInitialDamagedImmunityTime() {
        return initialDamagedImmunityTime;
    }

    public float GetInitialRespawnTime() {
        return initialRespawnTime;
    }

    public int GetInitialPlayerRespawnHealth() {
        return initialPlayerRespawnHealth;
    }

    public float GetInitialHpRegenTimer() {
        return initialHpRegenTimer;
    }

    public float GetInitialMoveSpeed() {
        return initialMoveSpeed;
    }

    public float GetInitialRunMaxTime() {
        return initialRunMaxTime;
    }

    public float GetInitialExhaustionTime() {
        return initialExhaustionTime;
    }

    public float GetInitialRunAccelerationFactor() {
        return initialRunAccelerationFactor;
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
                skillStat = runAccelerationFactor;
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

    #region BUFF PARAMETERS

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
        Debug.Log("BuffShootCooldown " + buffAmount);
        shootCooldownTime /= buffAmount;
    }

    public void DebuffShootCooldown(float buffAmount) {
        Debug.Log("DebuffShootCooldown " +buffAmount);
        shootCooldownTime *= buffAmount;
    }

    public void BuffRunAccelerationFactor(float buffAmount) {
        Debug.Log("BuffRunAccelerationFactor " + (initialRunAccelerationFactor-1) * buffAmount);
        runAccelerationFactor += (initialRunAccelerationFactor - 1) *  buffAmount;
    }

    public void BuffRunMaxTime(float buffAmount) {
        Debug.Log("BuffRunMaxTime " + buffAmount);
        runMaxTime += buffAmount;
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
}
