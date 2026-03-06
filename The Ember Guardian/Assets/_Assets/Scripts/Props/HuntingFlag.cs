using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HuntingFlag : MonoBehaviour
{
    [SerializeField] private bool isMaxHuntingLimit;
    [SerializeField] private HuntingFlag_CampDefined campDefinedHuntingFlag;
    [SerializeField] private HuntingFlag_PlayerDefined playerDefinedHuntingFlag;

    public static event EventHandler OnAnyHuntingFlagReset;

    private Vector3 destinationPosition;
    private bool playerManuallySetFlagPosition;
    private bool playerIsCarryingFlag;


    public event EventHandler OnPlayerResetManualHuntingLimit;

    private void Start() {
        Obstacle.OnAnyObstacleBuilt += Obstacle_OnAnyObstacleBuilt;
    }

    private void Obstacle_OnAnyObstacleBuilt(object sender, EventArgs e) {
        if(destinationPosition != null && campDefinedHuntingFlag.transform.position != destinationPosition) {
            TrySetCampHuntingLimit(destinationPosition);
        }
    }

    public void TrySetCampHuntingLimit(Vector3 position) {
        destinationPosition = position;

        // Vérifiez s'il y a un obstacle entre les positions
        Vector3 direction = position - campDefinedHuntingFlag.transform.position;
        float distance = direction.magnitude;

        // Raycast pour détecter les obstacles
        RaycastHit2D hit = Physics2D.Raycast(campDefinedHuntingFlag.transform.position, direction.normalized, distance, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null) {
            Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
            if (obstacle == null) return;
            if (!obstacle.GetBuilt()) return;
        };

        SetCampHuntingLimit(position);
    }

    public void SetCampHuntingLimit(Vector3 position) {
        campDefinedHuntingFlag.transform.position = position;

        if (playerManuallySetFlagPosition) {

            if (isMaxHuntingLimit) {
                if (position.x > playerDefinedHuntingFlag.transform.position.x) {
                    playerDefinedHuntingFlag.transform.position = position;
                }
            }
            else {
                if (position.x < playerDefinedHuntingFlag.transform.position.x) {
                    playerDefinedHuntingFlag.transform.position = position;
                }
            }

        }
        else {
            playerDefinedHuntingFlag.transform.position = position;
        }
    }

    public float GetCampHuntingLimit() {
        if(playerManuallySetFlagPosition || playerDefinedHuntingFlag.GetPlayerIsCarryingFlag()) {
            return playerDefinedHuntingFlag.transform.position.x;
        } else {
            return campDefinedHuntingFlag.transform.position.x;
        }
    }

    public void ResetPlayerManuallySetHuntingLimit() {
        playerManuallySetFlagPosition = false;
        playerIsCarryingFlag = false;
        OnPlayerResetManualHuntingLimit?.Invoke(this, EventArgs.Empty);
        OnAnyHuntingFlagReset?.Invoke(this, EventArgs.Empty);
    }

    public bool GetPlayerDefinedHuntingLimit() {
        return playerManuallySetFlagPosition;
    }

    public Vector3 GetCampDefinedHuntingFlagPosition() {
        return campDefinedHuntingFlag.transform.position;
    }

    public Vector3 GetPlayerDefinedHuntingFlagPosition() {
        return playerDefinedHuntingFlag.transform.position;
    }

    public bool SetPlayerDefinedHuntingLimit(bool defined) {
        return playerManuallySetFlagPosition = defined;
    }

    public void SetPlayerDefinedHuntingFlagPosition(Vector3 newPosition) {
        playerDefinedHuntingFlag.transform.position = newPosition;
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
