using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Collider2D floorCollider;

    [SerializeField] private float delayToActivateTeleporter;
    [SerializeField] private float delayToTeleportPlayerAnimation;

    private bool playerInTriggerArea;
    public event EventHandler OnPlayerEnteredTriggerArea;
    public event EventHandler OnPlayerExitedTriggerArea;
    public event EventHandler OnPlayerMovedOnTeleporter;
    public static event EventHandler OnAnyPlayerMovedOnTeleporter;
    public event EventHandler OnTeleporterActivated;
    public static event EventHandler OnPlayerTeleported;


    private void Start() {
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;

        floorCollider.enabled = false;
    }

    private void GameInput_OnPlayerInteractStarted(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;

        StartCoroutine(TeleportPlayer());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = true;
            OnPlayerEnteredTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = false;
            OnPlayerExitedTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerator TeleportPlayer() {
        floorCollider.enabled = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        OnPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);

        Debug.Log("OnPlayerMovedOnTeleporter");

        yield return new WaitForSeconds(delayToActivateTeleporter);
        OnTeleporterActivated?.Invoke(this, EventArgs.Empty);
        Debug.Log("OnTeleporterActivated");

        yield return new WaitForSeconds(delayToTeleportPlayerAnimation);
        OnPlayerTeleported?.Invoke(this, EventArgs.Empty);
        Debug.Log("OnPlayerTeleported");
    }
}
