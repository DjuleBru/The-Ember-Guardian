using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingFlag : MonoBehaviour
{
    [SerializeField] private bool isMaxHuntingLimit;
    [SerializeField] private Transform campDefinedHuntingFlag;
    [SerializeField] private Transform playerDefinedHuntingFlag;

    public static event EventHandler OnAnyHuntingFlagReset;

    private bool playerManuallySetFlagPosition;
    private bool playerIsCarryingFlag;

    public void SetCampHuntingLimit(Vector3 position) {
        campDefinedHuntingFlag.position = position;

        if(playerManuallySetFlagPosition) {

            if(isMaxHuntingLimit) {
                if(position.x > playerDefinedHuntingFlag.position.x) {
                    playerDefinedHuntingFlag.position = position;
                }
            } else {
                if (position.x < playerDefinedHuntingFlag.position.x) {
                    playerDefinedHuntingFlag.position = position;
                }
            }

        } else {
            playerDefinedHuntingFlag.position = position;
        }
    }

    public event EventHandler OnPlayerResetManualHuntingLimit;

    public float GetCampHuntingLimit() {
        if(playerManuallySetFlagPosition) {
            return playerDefinedHuntingFlag.position.x;
        } else {
            return campDefinedHuntingFlag.position.x;
        }
    }

    public void ResetPlayerManuallySetHuntingLimit() {
        playerManuallySetFlagPosition = false;
        OnPlayerResetManualHuntingLimit?.Invoke(this, EventArgs.Empty);
        OnAnyHuntingFlagReset?.Invoke(this, EventArgs.Empty);
    }

    public bool GetPlayerDefinedHuntingLimit() {
        return playerManuallySetFlagPosition;
    }

    public Vector3 GetCampDefinedHuntingFlagPosition() {
        return campDefinedHuntingFlag.position;
    }

    public bool SetPlayerDefinedHuntingLimit(bool defined) {
        return playerManuallySetFlagPosition = defined;
    }

    public bool GetPlayerCarryingFlag() {
        return playerIsCarryingFlag;
    }

    public void SetPlayerCarryingFlag(bool playerCarryingFlag) {
        playerIsCarryingFlag = playerCarryingFlag;
    }
} 
