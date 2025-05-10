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

    private bool justHitCreature;
    private float justHitCreatureTimer;

    private AudioSource audioSource;
    private StaticProjectile staticProjectile;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        staticProjectile = GetComponent<StaticProjectile>();

        if (!playSoundOnCreatureHit) return;
        staticProjectile.OnStaticProjectileHitCreature += StaticProjectile_OnStaticProjectileHitCreature;
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
        audioSource.PlayOneShot(projectileCreatureHitAudioClips[Random.Range(0, projectileCreatureHitAudioClips.Length)], projectileSFXVolumeMultiplier * sfxVolume);
    }

    public void TriggerProjectileSFX() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        audioSource.PlayOneShot(projectileSFXAudioClips[Random.Range(0, projectileSFXAudioClips.Length)], projectileSFXVolumeMultiplier * sfxVolume);
    }
}
