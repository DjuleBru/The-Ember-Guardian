using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSounds : SoundObject
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private float projectileInstantiatedVolumeMultiplier = 1f;
    [SerializeField] private float projectileHitVolumeMultiplier = 1f;

    private void Awake() {
        projectile.OnProjectileHit += Projectile_OnProjectileHit;
    }

    protected override void Start() {
        base.Start();
        PlaySound2D(projectile.GetProjectileSO().projectileInstantiatedAudioClips, projectileInstantiatedVolumeMultiplier);
    }

    private void Projectile_OnProjectileHit(object sender, System.EventArgs e) {
        PlaySound2D(projectile.GetProjectileSO().projectileHitAudioClips, projectileHitVolumeMultiplier);
    }
}
