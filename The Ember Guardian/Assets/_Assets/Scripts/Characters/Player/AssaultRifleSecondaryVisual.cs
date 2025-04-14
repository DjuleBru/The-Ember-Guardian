using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleSecondaryVisual : MonoBehaviour
{
    private AssaultRifleSecondaryAbility assaultRifleSecondaryAbility;
    [SerializeField] private ParticleSystem orbInfusedPS;

    private void Awake() {
        assaultRifleSecondaryAbility = GetComponent<AssaultRifleSecondaryAbility>();
    }

    private void Start() {
        assaultRifleSecondaryAbility.OnInfusedOrbAmmoClipEmpty += PlayerShoot_OnInfusedOrbAmmoClipEmpty;
        assaultRifleSecondaryAbility.OnPlayerInfusedOrbInAmmoClip += PlayerShoot_OnPlayerInfusedOrbInAmmoClip;
    }

    private void PlayerShoot_OnPlayerInfusedOrbInAmmoClip(object sender, System.EventArgs e) {
        orbInfusedPS.Play();
    }

    private void PlayerShoot_OnInfusedOrbAmmoClipEmpty(object sender, System.EventArgs e) {
        orbInfusedPS.Stop();
    }
}
