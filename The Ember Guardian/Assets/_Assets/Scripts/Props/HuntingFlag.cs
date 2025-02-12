using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HuntingFlag : MonoBehaviour
{
    [SerializeField] private bool isMaxHuntingLimit;
    [SerializeField] private Transform campDefinedHuntingFlag;
    [SerializeField] private Transform playerDefinedHuntingFlag;

    public static event EventHandler OnAnyHuntingFlagReset;

    private Vector3 destinationPosition;
    private bool playerManuallySetFlagPosition;
    private bool playerIsCarryingFlag;


    public event EventHandler OnPlayerResetManualHuntingLimit;

    private void Start() {
        Obstacle.OnAnyObstacleBuilt += Obstacle_OnAnyObstacleBuilt;
    }


    private void Obstacle_OnAnyObstacleBuilt(object sender, EventArgs e) {
        if(destinationPosition != null && campDefinedHuntingFlag.position != destinationPosition) {
            TrySetCampHuntingLimit(destinationPosition);
        }
    }

    public void TrySetCampHuntingLimit(Vector3 position) {
        destinationPosition = position;

        // Vérifiez s'il y a un obstacle entre les positions
        Vector3 direction = position - campDefinedHuntingFlag.position;
        float distance = direction.magnitude;

        // Raycast pour détecter les obstacles
        RaycastHit2D hit = Physics2D.Raycast(campDefinedHuntingFlag.position, direction.normalized, distance, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null) {
            Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
            if (!obstacle.GetBuilt()) return;
        };

        SetCampHuntingLimit(position);
    }

    public void SetCampHuntingLimit(Vector3 position) {
        campDefinedHuntingFlag.position = position;

        if (playerManuallySetFlagPosition) {

            if (isMaxHuntingLimit) {
                if (position.x > playerDefinedHuntingFlag.position.x) {
                    playerDefinedHuntingFlag.position = position;
                }
            }
            else {
                if (position.x < playerDefinedHuntingFlag.position.x) {
                    playerDefinedHuntingFlag.position = position;
                }
            }

        }
        else {
            playerDefinedHuntingFlag.position = position;
        }
    }

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

    private void OnDestroy() {
        Obstacle.OnAnyObstacleBuilt -= Obstacle_OnAnyObstacleBuilt;
    }
} 
