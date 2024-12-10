using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchant : MonoBehaviour
{
    private bool playerInTriggerArea;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerInteractedWithHubMerchant;

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        OnPlayerInteractedWithHubMerchant?.Invoke(this, EventArgs.Empty);

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Player>() != null) {
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() != null) {
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }
}
