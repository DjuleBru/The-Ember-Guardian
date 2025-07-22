using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerControlFlag : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer pickUpSpriteRenderer;
    [SerializeField] private SpriteRenderer flagSpriteRenderer;
    [SerializeField] private Sprite controlActiveSprite;
    [SerializeField] private Sprite controlInactiveSprite;

    private bool controlActive;
    private bool playerInTriggerArea;
    private bool playerIsCarryingFlag;

    public static event EventHandler OnAnyControlFlagPickedUp;
    public static event EventHandler OnAnyControlFlagDropped;
    public static event EventHandler OnAnyPlayerTriggeredIn;


    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickUpSpriteRenderer.enabled = false;
        flagSpriteRenderer.sprite = controlInactiveSprite;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnCommandWorkerPerformed += GameInput_OnCommandWorkerPerformed;
    }

    private void GameInput_OnCommandWorkerPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        controlActive = !controlActive;
        if(controlActive) {
            flagSpriteRenderer.sprite = controlActiveSprite;
        } else {
            flagSpriteRenderer.sprite = controlInactiveSprite;
        }

    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {

        if (playerIsCarryingFlag) {
            if (!Player.Instance.GetInNoOtherObjectTriggerArea()) return;
            DropFlag();
        }
        else {

            if (!playerInTriggerArea) return;
            StartCarryingFlag();
        }
    }

    private void StartCarryingFlag() {
        playerIsCarryingFlag = true;

        Vector3 newScale = Vector3.one;
        newScale.x = -1f;

        transform.position = Player.Instance.GetCarryingFlagPosition().position;
        transform.SetParent(Player.Instance.GetCarryingFlagPosition());

        OnAnyControlFlagPickedUp?.Invoke(this, EventArgs.Empty);

        spriteRenderer.transform.localScale = newScale;

        pickUpSpriteRenderer.enabled = false;
    }

    private void DropFlag() {
        playerIsCarryingFlag = false;
        transform.SetParent(null);
        transform.position = new Vector3(transform.position.x, 0,0);
        OnAnyControlFlagDropped?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.gameObject.GetComponent<Player>() != null) {
            OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

            if (!WorkerStats.Instance.GetInteractionWithWorkersUnlocked()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            spriteRenderer.material.SetFloat("_Glow", .2f);
            pickUpSpriteRenderer.enabled = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!WorkerStats.Instance.GetInteractionWithWorkersUnlocked()) return;

        if (collision.gameObject.GetComponent<Player>() != null) {

            playerInTriggerArea = false;
            spriteRenderer.material.SetFloat("_Glow", 0f);

            if (playerIsCarryingFlag) return;
            Player.Instance.SetCarryinhOtherObject(false);
            pickUpSpriteRenderer.enabled = false;
        }
    }
}
