using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingFlag_PlayerDefined : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    private HuntingFlag huntingFlag;
    private bool playerInTriggerArea;
    private bool playerCarryingFlag;

    public static event EventHandler OnAnyHuntingFlagPickedUp;
    public static event EventHandler OnAnyHuntingFlagNewPositionSet;
    public static event EventHandler OnAnyPlayerTriggeredIn;

    private void Awake() {
        huntingFlag = GetComponentInParent<HuntingFlag>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        huntingFlag.OnPlayerResetManualHuntingLimit += HuntingFlag_OnPlayerResetManualHuntingLimit;
    }

    private void Update() {
        if (!huntingFlag.GetPlayerCarryingFlag()) return;

        if(CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            huntingFlag.ResetPlayerManuallySetHuntingLimit();
        }
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if(playerCarryingFlag) {
            Vector3 currentPosition = new Vector3(Player.Instance.transform.position.x, 0f, 0f);
            SetNewFlagPosition(currentPosition);
        } else {
            if (!playerInTriggerArea) return;
            StartCarryingFlag();
        }
    }

    private void StartCarryingFlag() {
        playerCarryingFlag = true;
        transform.position = Player.Instance.GetCarryingFlagPosition().position;
        transform.SetParent(Player.Instance.GetCarryingFlagPosition());

        huntingFlag.SetPlayerCarryingFlag(true);
        OnAnyHuntingFlagPickedUp?.Invoke(this, EventArgs.Empty);
    }

    private void HuntingFlag_OnPlayerResetManualHuntingLimit(object sender, System.EventArgs e) {
        ResetFlagPosition();
    }

    private void SetNewFlagPosition(Vector3 position) {
        playerCarryingFlag = false;
        huntingFlag.SetPlayerDefinedHuntingLimit(true);
        huntingFlag.SetPlayerCarryingFlag(false);
        transform.SetParent(huntingFlag.transform);
        transform.position = position;

        OnAnyHuntingFlagNewPositionSet?.Invoke(this, EventArgs.Empty);
    }

    private void ResetFlagPosition() {
        transform.localScale = new Vector3(-1, 1, 1);
        huntingFlag.SetPlayerDefinedHuntingLimit(false);
        transform.SetParent(huntingFlag.transform);
        transform.position = huntingFlag.GetCampDefinedHuntingFlagPosition();
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if(collision.gameObject.GetComponent<Player>() != null) {
            OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            spriteRenderer.material.SetFloat("_Glow", .2f);

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

        if (collision.gameObject.GetComponent<Player>() != null) {

            playerInTriggerArea = false;
            spriteRenderer.material.SetFloat("_Glow", 0f);

            if (huntingFlag.GetPlayerCarryingFlag()) return;
            Player.Instance.SetCarryinhOtherObject(false);
        }
    }
}
