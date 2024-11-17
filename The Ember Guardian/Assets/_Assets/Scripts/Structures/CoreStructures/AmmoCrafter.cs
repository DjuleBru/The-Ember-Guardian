using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrafter : Structure
{
    private float ammoCraftTimer;
    [SerializeField] private float ammoCraftTime = 45f;
    [SerializeField] private Transform ammoSpawnPoint;
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

            StartCoroutine(CollectAmmoFromCrafter(.2f));

        }
    }

    private IEnumerator CollectAmmoFromCrafter(float delayBetweenAmmoInstantiation) {
        craftedAmmo = false;
        OnPlayerCollectedAmmo?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < ammoCraftAmount; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.ammo), ammoSpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(1.5f);
            collectible.ApplyRandomForce(-1, 1, 3, 5);

            yield return new WaitForSeconds(delayBetweenAmmoInstantiation);
        }
    }

    public int GetAmmoCraftAmount() {
        return ammoCraftAmount;
    }

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (ammoCraftTimer / ammoCraftTime);
    }

}
