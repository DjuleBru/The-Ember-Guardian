using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectileSounds : SoundObject
{
    [SerializeField] private AudioClip[] projectileSFXAudioClips;
    [SerializeField] private float projectileSFXVolumeMultiplier;
    [SerializeField] private bool playSoundOnCreatureHit;
    [SerializeField] private AudioClip[] projectileCreatureHitAudioClips;
    [SerializeField] private float minDelayBetweenCreatureHitPlays = .2f;
    [SerializeField] private AudioClip[] projectileExplosionAudioClips;
    [SerializeField] private float projectileCreatureHitVolumeMultiplier;

    private bool justHitCreature;
    private float justHitCreatureTimer;

    private AudioSource audioSource;
    private StaticProjectile staticProjectile;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        staticProjectile = GetComponent<StaticProjectile>();
        staticProjectile.OnTrapTriggered += StaticProjectile_OnTrapTriggered;

        if (!playSoundOnCreatureHit) return;
        staticProjectile.OnStaticProjectileHitCreature += StaticProjectile_OnStaticProjectileHitCreature;
    }

    private void StaticProjectile_OnTrapTriggered(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        audioSource.PlayOneShot(projectileExplosionAudioClips[Random.Range(0, projectileExplosionAudioClips.Length)], projectileSFXVolumeMultiplier * sfxVolume);
    }

    private void Update() {
        if (justHitCreature) {
            justHitCreatureTimer -= Time.deltaTime;
            if (justHitCreatureTimer < 0) {
                justHitCreature = false;
            }
        }
    }

    private void StaticProjectile_OnStaticProjectileHitCreature(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        if (projectileCreatureHitAudioClips.Length == 0) return;

        audioSource.PlayOneShot(projectileCreatureHitAudioClips[Random.Range(0, projectileCreatureHitAudioClips.Length)], projectileCreatureHitVolumeMultiplier * sfxVolume);
    }

    public void TriggerProjectileSFX() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        if (projectileSFXAudioClips.Length == 0) return;

        AudioClip audioClip = projectileSFXAudioClips[Random.Range(0, projectileSFXAudioClips.Length)];
        audioSource.PlayOneShot(audioClip, projectileSFXVolumeMultiplier * sfxVolume);
    }
}
