using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleSecondaryAbilitySounds : SoundObject
{
    private AssaultRifleSecondaryAbility assaultRifleSecondaryAbility;

    [SerializeField] private AudioClip infuseOrbAudioClip;
    [SerializeField] private AudioClip shootWithInfusedOrbAudioClip;
    private bool orbInfused;

    protected override void Start() {
        base.Start();

        assaultRifleSecondaryAbility = GetComponent<AssaultRifleSecondaryAbility>();
        assaultRifleSecondaryAbility.OnInfusedOrbAmmoClipEmpty += AssaultRifleSecondaryAbility_OnInfusedOrbAmmoClipEmpty;
        assaultRifleSecondaryAbility.OnPlayerInfusedOrbInAmmoClip += AssaultRifleSecondaryAbility_OnPlayerInfusedOrbInAmmoClip;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (orbInfused) {
            PlaySound2D(shootWithInfusedOrbAudioClip);
        }
    }

    private void AssaultRifleSecondaryAbility_OnPlayerInfusedOrbInAmmoClip(object sender, System.EventArgs e) {
        orbInfused = true;
        PlaySound2D(infuseOrbAudioClip, 2f);
    }

    private void AssaultRifleSecondaryAbility_OnInfusedOrbAmmoClipEmpty(object sender, System.EventArgs e) {
        orbInfused = false;
    }
}
