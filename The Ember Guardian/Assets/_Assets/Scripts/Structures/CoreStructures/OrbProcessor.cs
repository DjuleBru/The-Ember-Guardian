using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbProcessor : Structure
{
    private float orbCraftTimer;
    [SerializeField] private float orbCraftTime = 45f;
    [SerializeField] private Transform orbSpawnPoint;

    private OrbProcessorOrbCollider orbProcessorOrbCollider;
    private int smallOrbsToBigOrbRatio = 5;
    private int smallOrbCapacity = 5;
    private int smallOrbsInProcessor;
    private int orbCraftAmount;

    private bool craftingOrb;
    private bool craftedOrb;

    public event EventHandler OnOrbCraftingStarted;
    public event EventHandler OnOrbCraftingEnded;
    public event EventHandler OnPlayerCollectedOrb;

    protected override void Awake() {
        base.Awake();
        orbProcessorOrbCollider = GetComponentInChildren<OrbProcessorOrbCollider>();
    }

    protected override void Start() {
        base.Start();
        ActivateStructureFunctionInteraction(true);

        orbProcessorOrbCollider.OnOrbFellInOrbProcessor += OrbProcessorOrbCollider_OnOrbFellInOrbProcessor;
    }

    private void Update() {
        if (!craftingOrb) return;

        orbCraftTimer -= Time.deltaTime;

        if (orbCraftTimer < 0) {
            OnOrbCraftingEnded?.Invoke(this, EventArgs.Empty);
            craftingOrb = false;
            craftedOrb = true;
        }
    }

    protected override void TriggerStructureFunction() {
        if (!craftedOrb) {

            craftingOrb = true;
            orbCraftTimer = orbCraftTime;
            OnOrbCraftingStarted?.Invoke(this, EventArgs.Empty);

        }
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;

        if (!craftedOrb && !craftingOrb) {

            payOrbsUI.SetPlayerInteracting(true);

        }
        else {

            if (!craftedOrb) return;
            // Ammo has not finished crafting

            StartCoroutine(CollectOrbsFromCrafter(.2f));

        }
    }

    private void OrbProcessorOrbCollider_OnOrbFellInOrbProcessor(object sender, EventArgs e) {
        smallOrbsInProcessor++;
        if(smallOrbsInProcessor == smallOrbCapacity) {

        }
    }

    private IEnumerator CollectOrbsFromCrafter(float delayBetweenOrbInstantiation) {
        craftedOrb = false;
        smallOrbsInProcessor = 0;
        OnPlayerCollectedOrb?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < orbCraftAmount; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.bigBlueOrb), orbSpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(1.5f);
            collectible.ApplyRandomForce(-1, 1, 6, 9);

            yield return new WaitForSeconds(delayBetweenOrbInstantiation);
        }
    }

    public int GetOrbCraftAmount() {
        return orbCraftAmount;
    }

    public bool GetCraftingOrb() {
        return craftingOrb;
    }

    public float GetOrbCraftTimerNormalized() {
        return 1 - (orbCraftTimer / orbCraftTime);
    }
}
