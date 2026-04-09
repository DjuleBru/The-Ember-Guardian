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
        projectile.OnProjectileInitialized += Projectile_OnProjectileInitialized;
    }

    private void Projectile_OnProjectileInitialized(object sender, System.EventArgs e) {
        if(audioSource2D == null) {
            audioSource2D = GetComponent<AudioSource>();
        }
        if (projectile.GetProjectileSO() == null) return;
        PlaySound2D(projectile.GetProjectileSO().projectileInstantiatedAudioClips, projectileInstantiatedVolumeMultiplier);
    }

    protected override void Start() {
        // Projectile sounds has not subscribed to initialize : play instantiate sound
        base.Start();

        if (audioSource2D == null) {
            audioSource2D = GetComponent<AudioSource>();
        }

        if (projectile.GetProjectileSO() == null) return;
        PlaySound2D(projectile.GetProjectileSO().projectileInstantiatedAudioClips, projectileInstantiatedVolumeMultiplier);
    }

    private void Projectile_OnProjectileHit(object sender, System.EventArgs e) {
        if (projectile.GetProjectileSO() == null) return;
        PlaySound2D(projectile.GetProjectileSO().projectileHitAudioClips, projectileHitVolumeMultiplier);
    }
}
