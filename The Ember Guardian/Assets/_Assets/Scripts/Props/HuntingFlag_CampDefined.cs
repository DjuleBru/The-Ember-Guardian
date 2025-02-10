using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingFlag_CampDefined : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private HuntingFlag huntingFlag;
    private Color initialColor;
    [SerializeField] private Color playerInteractColor;
    private bool playerInTriggerArea;


    private void Awake() {
        huntingFlag = GetComponentInParent<HuntingFlag>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;

        huntingFlag.ResetPlayerManuallySetHuntingLimit();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;
            if (huntingFlag.GetPlayerCarryingFlag()) return;
            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            spriteRenderer.color = playerInteractColor;

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {

            if (huntingFlag.GetPlayerCarryingFlag()) return;
            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            Player.Instance.SetCarryinhOtherObject(false);
            playerInTriggerArea = false;
            spriteRenderer.color = initialColor;
            
        }
    }
}
