using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingFlag_CampDefined : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer resetSpriteRenderer;

    private HuntingFlag huntingFlag;
    private Color initialColor;
    [SerializeField] private Color playerInteractColor;
    private bool playerInTriggerArea;


    private void Awake() {
        huntingFlag = GetComponentInParent<HuntingFlag>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
        resetSpriteRenderer.enabled = false;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;

        huntingFlag.ResetPlayerManuallySetHuntingLimit();
        resetSpriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;
            if (huntingFlag.GetPlayerCarryingFlag()) return;
            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            spriteRenderer.color = playerInteractColor;
            resetSpriteRenderer.enabled = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = false;

            if (huntingFlag.GetPlayerCarryingFlag()) return;

            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            Player.Instance.SetCarryinhOtherObject(false);
            spriteRenderer.color = initialColor;
            resetSpriteRenderer.enabled = false;

        }
    }
}
