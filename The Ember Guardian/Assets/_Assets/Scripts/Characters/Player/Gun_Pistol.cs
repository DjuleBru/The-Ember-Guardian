using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Pistol : Gun
{
    private float gunSilencerDamageReduction = 1.3f;

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;
    }

    private void PlayerShoot_OnPlayerSwitchedFireMode(object sender, EventArgs e) {
        CheckPlayerHasSilencer();
    }

    protected void CheckPlayerHasSilencer() {
        if (!gunActive) return;

        if (PlayerShoot.Instance.GetSilencerActive()) {
            shootCreatureHearMultiplier = 1.15f;
            DebuffBulletDamage(gunSilencerDamageReduction, false);
        }
        else {
            shootCreatureHearMultiplier = gunSO.shootCreatureHearMultiplier;
            BuffBulletDamage(gunSilencerDamageReduction, false);
        }
    }

}
