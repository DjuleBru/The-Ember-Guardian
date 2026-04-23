using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HuntingFlag_CampDefined : MonoBehaviour
{
    private SpriteRenderer poleSpriteRenderer;
    [SerializeField] private SpriteRenderer flagSpriteRenderer;
    [SerializeField] private SpriteRenderer resetSpriteRenderer;
    [SerializeField] private Image pickUpHoldToFillImage;
    [SerializeField] private GameObject pickUpHoldToFillGO;

    private HuntingFlag huntingFlag;
    private Color initialColor;
    [SerializeField] private Color playerInteractColor;
    private bool playerInTriggerArea;

    private float holdDuration = .4f;
    private float holdTimer;
    private bool isHolding;

    private void Awake() {
        huntingFlag = GetComponentInParent<HuntingFlag>();
        poleSpriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = poleSpriteRenderer.color;
        resetSpriteRenderer.enabled = false;
        pickUpHoldToFillGO.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        isHolding = false;
    }

    private void Update() {
        HandleHoldInteraction();
    }

    private void HandleHoldInteraction() {
        if (!isHolding) {
            holdTimer = 0f;
            pickUpHoldToFillImage.fillAmount = 0f;
            return;
        }
        
        holdTimer += Time.deltaTime;
        pickUpHoldToFillImage.fillAmount = holdTimer / holdDuration;

        if (holdTimer < holdDuration) return;

        huntingFlag.ResetPlayerManuallySetHuntingLimit();
        resetSpriteRenderer.enabled = false;
        pickUpHoldToFillGO.SetActive(false);

        // reset
        isHolding = false;
        holdTimer = 0f;
        pickUpHoldToFillImage.fillAmount = 0f;
        
    }


    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;

        isHolding = true;
        holdTimer = 0f;
        pickUpHoldToFillImage.fillAmount = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;
            if ((!huntingFlag.GetPlayerDefinedHuntingLimit())) return;
            if (huntingFlag.GetPlayerCarryingFlag()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            poleSpriteRenderer.color = playerInteractColor;
            flagSpriteRenderer.color = playerInteractColor;
            resetSpriteRenderer.enabled = true;
            pickUpHoldToFillGO.SetActive(true);

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = false;

            if (huntingFlag.GetPlayerCarryingFlag()) return;

            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            Player.Instance.SetCarryinhOtherObject(false);
            poleSpriteRenderer.color = initialColor;
            flagSpriteRenderer.color = initialColor;
            resetSpriteRenderer.enabled = false;
            pickUpHoldToFillGO.SetActive(false);

        }
    }
}
