using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSecondaryAbility : MonoBehaviour
{
    protected Gun gun;
    protected bool secondaryAbilityActive;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();
    }

    protected virtual void Start() {
        GameInput.Instance.OnWeaponSecondaryAbilityCanceled += GameInput_OnWeaponSecondaryAbilityCanceled;
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilityPerformed;
    }

    protected void GameInput_OnWeaponSecondaryAbilityPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        bool secondaryAbilityUnlocked = gun.GetSecondaryAbilityUnlocked();

        if (!secondaryAbilityUnlocked) return;
        if (!PlayerShoot.Instance.GetCanShoot()) return;
        if (PlayerShoot.Instance.GetReloading()) return;
        PerformSecondaryAbility();
    }

    protected virtual void GameInput_OnWeaponSecondaryAbilityCanceled(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        if (secondaryAbilityActive) {

            CancelSecondaryAbility();

        }

    }
    protected virtual void PerformSecondaryAbility() {

    }

    protected virtual void CancelSecondaryAbility() {

    }

}
