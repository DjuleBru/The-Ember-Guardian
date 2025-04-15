using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleSecondaryVisual : MonoBehaviour
{
    private AssaultRifleSecondaryAbility assaultRifleSecondaryAbility;
    [SerializeField] private ParticleSystem orbInfusedPS;
    [SerializeField] private SpriteRenderer orbInfusedSpriteRenderer;
    [SerializeField] private MMF_Player orbInfusedFeedbacks;

    private void Awake() {
        assaultRifleSecondaryAbility = GetComponent<AssaultRifleSecondaryAbility>();
        orbInfusedSpriteRenderer.enabled = false;
    }

    private void Start() {
        assaultRifleSecondaryAbility.OnInfusedOrbAmmoClipEmpty += PlayerShoot_OnInfusedOrbAmmoClipEmpty;
        assaultRifleSecondaryAbility.OnPlayerInfusedOrbInAmmoClip += PlayerShoot_OnPlayerInfusedOrbInAmmoClip;
    }

    private void PlayerShoot_OnPlayerInfusedOrbInAmmoClip(object sender, System.EventArgs e) {
        orbInfusedPS.Play();
        orbInfusedSpriteRenderer.enabled = true;
        orbInfusedFeedbacks.PlayFeedbacks();
    }

    private void PlayerShoot_OnInfusedOrbAmmoClipEmpty(object sender, System.EventArgs e) {
        orbInfusedPS.Stop();
        orbInfusedSpriteRenderer.enabled = false;
    }
}
