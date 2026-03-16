using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileSound : SoundObject
{
    [SerializeField] private AudioClip[] bounceOnCreatureAudioClips;
    [SerializeField] private AudioClip[] bounce1AudioClips;
    [SerializeField] private AudioClip[] bounce2AudioClips;
    [SerializeField] private AudioClip[] bounce3AudioClips;
    [SerializeField] private AudioClip[] explosionAudioClips;
    [SerializeField] private float explosionVolumeMultiplier = 1.0f;
    [SerializeField] private GunProjectile_BounceHandler gunProjectile_BounceHandler;

    private GunProjectile gunProjectile;
    private int groundBounceAmount;

    protected void Awake() {
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;
        gunProjectile_BounceHandler.OnProjectileBouncedOnGround += GunProjectile_BounceHandler_OnProjectileBouncedOnGround;
        gunProjectile_BounceHandler.OnProjectileBouncedOnCreature += GunProjectile_BounceHandler_OnProjectileBouncedOnCreature;

    }

    private void GunProjectile_BounceHandler_OnProjectileBouncedOnCreature(object sender, System.EventArgs e) {
        PlaySound2D(bounceOnCreatureAudioClips);
    }

    private void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        Debug.Log("GunProjectile_OnProjectileExploded");
        PlaySound2D(explosionAudioClips, explosionVolumeMultiplier);
    }

    private void GunProjectile_BounceHandler_OnProjectileBouncedOnGround(object sender, System.EventArgs e) {
        groundBounceAmount++;
        if(groundBounceAmount == 1) {
            PlaySound2D(bounce1AudioClips, 2f);
        }
        if (groundBounceAmount == 2) {
            PlaySound2D(bounce2AudioClips, 2f);
        }
        if (groundBounceAmount >= 3) {
            PlaySound2D(bounce3AudioClips, 2f);
        }
    }
}
