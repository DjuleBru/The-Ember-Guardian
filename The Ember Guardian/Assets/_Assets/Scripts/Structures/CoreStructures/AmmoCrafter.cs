using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrafter : Structure
{
    private float ammoCraftTimer;
    [SerializeField] private float ammoCraftTime = 45f;
    private int ammoCraftAmount = 3;

    private bool craftingAmmo;
    private bool craftedAmmo;

    public event EventHandler OnAmmoCraftingStarted;
    public event EventHandler OnAmmoCraftingEnded;
    public event EventHandler OnPlayerCollectedAmmo;

    protected override void Start() {
        base.Start();
        ActivateStructureFunctionInteraction(true);
    }

    private void Update() {
        if (!craftingAmmo) return;

        ammoCraftTimer -= Time.deltaTime;

        if(ammoCraftTimer < 0) {
            OnAmmoCraftingEnded?.Invoke(this, EventArgs.Empty);
            craftingAmmo = false;
            craftedAmmo = true;
        }
    }

    protected override void TriggerStructureFunction() {
        if(!craftedAmmo) {

            craftingAmmo = true;
            ammoCraftTimer = ammoCraftTime;
            OnAmmoCraftingStarted?.Invoke(this, EventArgs.Empty);

        } else {

            CollectAmmoFromCrafter();

        }
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;

        if(!craftedAmmo && !craftingAmmo) {

            payOrbsUI.SetPlayerInteracting(true);

        } else {

            if (!craftedAmmo) return;
            // Ammo has not finished crafting

            if (PlayerShoot.Instance.GetCurrentAmmo() == PlayerShoot.Instance.GetMaxAmmo()) return;
            // Player has max ammo

            CollectAmmoFromCrafter();

        }
    }

    private void CollectAmmoFromCrafter() {
        craftedAmmo = false;
        PlayerShoot.Instance.AddAmmo(ammoCraftAmount);
        OnPlayerCollectedAmmo?.Invoke(this, EventArgs.Empty);
    }

    public int GetAmmoCraftAmount() {
        return ammoCraftAmount;
    }

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (ammoCraftTimer / ammoCraftTime);
    }

}
