using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleSecondaryAbility : GunSecondaryAbility {

    public static AssaultRifleSecondaryAbility Instance;

    [SerializeField] private Transform smallOrbSpawnPosition;
    [SerializeField] private Transform smallOrbDestinationPosition;
    [SerializeField] private ParticleSystem secondaryBulletPS;

    private bool ammoClipInfusedWithOrb;
    private bool homingBulletsActive;
    private bool playerJustActivatedSecondary;
    private float playerJustActivatedSecondaryTimer;

    private bool transferringAmmoFromBag;
    private float transferringAmmoFromBagTimer;

    private Collectible smallOrbBeingTransferred;
    private bool smallOrbIsBeingTransferred;

    public event EventHandler OnPlayerInfusedOrbInAmmoClip;
    public event EventHandler OnInfusedOrbAmmoClipEmpty;

    protected override void Awake() {
        base.Awake();
        Instance = this;
    }

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += PlayerInventoryUI_OnCurrencyDropped;
        PlayerAim.Instance.OnXAimDirChanged += PlayerAim_OnXAimDirChanged;
    }

    private void PlayerAim_OnXAimDirChanged(object sender, EventArgs e) {
        if(PlayerAim.Instance.GetAimDir().x > 0) {
            secondaryBulletPS.transform.localScale = Vector3.one;
        } else {
            Vector3 newScale = Vector3.one;
            newScale.y = -1;
            secondaryBulletPS.transform.localScale = newScale;
        }
    }

    private void PlayerShoot_OnPlayerShot(object sender, EventArgs e) {
        if(ammoClipInfusedWithOrb) {
            if(PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                secondaryBulletPS.Emit(1);
            }

            if(PlayerShoot.Instance.GetSecondSecondaryAbilityEquipped()) {
                homingBulletsActive = true;
            }
        }

        if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
            ammoClipInfusedWithOrb = false;
            homingBulletsActive = false;
            OnInfusedOrbAmmoClipEmpty?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void Update() {
        if (transferringAmmoFromBag) return;

        if (playerJustActivatedSecondary) {
            playerJustActivatedSecondaryTimer += Time.deltaTime;
            if (playerJustActivatedSecondaryTimer > .35f) {
                playerJustActivatedSecondary = false;
                StartTransferringOrbFromBagInGun();
            }
        }
    }

    private void PlayerInventoryUI_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            smallOrbBeingTransferred = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.smallBlueOrb), smallOrbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
            smallOrbBeingTransferred.SetMovingForPayment(true, 5f, smallOrbDestinationPosition);
            smallOrbBeingTransferred.SetDestroyOnDestinationReached(OnBlueOrbInsertedInGun);
            smallOrbIsBeingTransferred = true;
        }
    }

    protected void OnBlueOrbInsertedInGun() {
        OnPlayerInfusedOrbInAmmoClip?.Invoke(this, EventArgs.Empty);
        ammoClipInfusedWithOrb = true;
        smallOrbIsBeingTransferred = false;
    }

    private void StartTransferringOrbFromBagInGun() {
        transferringAmmoFromBagTimer = 0;
        transferringAmmoFromBag = true;
        TransferNextOrbFromBag();
    }

    private void TransferNextOrbFromBag() {
        int smallOrbAmountInBag = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb).Count;

        if (smallOrbAmountInBag > 0 && gun.GetCurrentAmmoClip() < gun.GetMaxAmmo()) {
            UICurrencyManager.PlayerInventoryUI.DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.smallBlueOrb);
        }

    }

    protected override void PerformSecondaryAbility() {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        playerJustActivatedSecondary = true;
        playerJustActivatedSecondaryTimer = 0;


        if (ammoClipInfusedWithOrb) return;
        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb).Count > 0) {
            UICurrencyManager.PlayerInventoryUI.DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.smallBlueOrb);
        }
    }

    protected override void GameInput_OnWeaponSecondaryAbilityCanceled(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        if(smallOrbIsBeingTransferred) {
            smallOrbIsBeingTransferred = false;
            smallOrbBeingTransferred.SetMovingForPayment(false);
        }

        if (secondaryAbilityActive) {

            CancelSecondaryAbility();

        }

    }

    public bool GetHomingBulletsActive() {
        return homingBulletsActive;
    }

}
