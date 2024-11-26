using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    #region MOVEMENT
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    [SerializeField] private float initialMoveSpeed;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    [SerializeField] private float initialRunMaxTime;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    [SerializeField] private float initialExhaustionTime;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    [SerializeField] private float initialRunAccelerationFactor;

    private float moveSpeed;
    private float runMaxTime;
    private float exhaustionTime;
    private float runAccelerationFactor;

    #endregion

    # region Health
    [BoxGroup("Health")]
    [LabelWidth(125)]
    [SerializeField] private int initialMaxPlayerHP = 7;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    [SerializeField] private float initialDamagedImmunityTime = 1.5f;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    [SerializeField] private float initialRespawnTime = 5f;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    [SerializeField] private int initialPlayerRespawnHealth = 3;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    [SerializeField] private float initialHpRegenTimer = 0f;

    private int maxPlayerHP;
    private float damagedImmunityTime;
    private float respawnTime;
    private int playerRespawnHealth;
    private float hpRegenTimer;


    #endregion

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }

    private void InitializeParameters() {
        moveSpeed = initialMoveSpeed;
        runMaxTime = initialRunMaxTime;
        exhaustionTime = initialExhaustionTime;
        runAccelerationFactor = initialRunAccelerationFactor;

        maxPlayerHP = initialMaxPlayerHP;
        damagedImmunityTime += initialDamagedImmunityTime;
        respawnTime = initialRespawnTime;
        playerRespawnHealth = initialPlayerRespawnHealth;
        hpRegenTimer += initialHpRegenTimer;
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

    public float GetHpRegenTimer() {
        return hpRegenTimer;
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

    #region BUFF PARAMETERS

    public void BuffMoveSpeed(float buffAmount) {
        Debug.Log("BuffMoveSpeed " + initialMoveSpeed * buffAmount);
        moveSpeed += initialMoveSpeed * buffAmount;
    }

    public void BuffRunAccelerationFactor(float buffAmount) {
        Debug.Log("BuffRunAccelerationFactor " + (initialRunAccelerationFactor-1) * buffAmount);
        runAccelerationFactor += (initialRunAccelerationFactor - 1) *  buffAmount;
    }

    public void BuffRunMaxTime(float buffAmount) {
        Debug.Log("BuffRunMaxTime " + buffAmount);
        runMaxTime += buffAmount;
    }

    #endregion
}
