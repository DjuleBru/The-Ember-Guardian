using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Pistol : Gun
{
    private float gunSilencerDamageReduction = 1.3f;

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnWeaponSecondaryAbilityStarted += PlayerShoot_OnWeaponSecondaryAbilityStarted;
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityStarted(object sender, EventArgs e) {
        CheckPlayerHasSilencer();
    }

    protected void CheckPlayerHasSilencer() {
        if (!gunActive) return;

        if (PlayerShoot.Instance.GetSilencerActive()) {
            shootCreatureHearMultiplier = 1;
            DebuffBulletDamage(gunSilencerDamageReduction);
        }
        else {
            shootCreatureHearMultiplier = gunSO.shootCreatureHearMultiplier;
            BuffBulletDamage(gunSilencerDamageReduction);
        }
    }

}
