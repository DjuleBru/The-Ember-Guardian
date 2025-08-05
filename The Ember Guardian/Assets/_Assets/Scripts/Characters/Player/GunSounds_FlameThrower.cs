using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds_FlameThrower : GunSounds
{

    [SerializeField] private AudioSource shootAudioSource;
    [SerializeField] private AudioClip[] startShotAudioClips;
    [SerializeField] private AudioClip[] shootLoopAudioClips;
    [SerializeField] private float volumeMultiplier;

    private bool shooting;

    protected override void Awake() {
        base.Awake();
        PlayerShoot.Instance.OnShotStartedLoading += PlayerShoot_OnShotStartedLoading;
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;

        shootAudioSource.volume = sfxVolume * volumeMultiplier;
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (shooting) return;

        shootAudioSource.clip = shootLoopAudioClips[UnityEngine.Random.Range(0, shootLoopAudioClips.Length)];
        shootAudioSource.Play();
        shootAudioSource.loop = true;
        shooting = true;
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        shootAudioSource.Stop();
        shooting = false;
    }

    private void PlayerShoot_OnShotStartedLoading(object sender, System.EventArgs e) {
        shootAudioSource.clip = startShotAudioClips[UnityEngine.Random.Range(0, startShotAudioClips.Length)];
        shootAudioSource.loop = false;
        shootAudioSource.Play();
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        base.SettingsManager_OnSfxVolumeChanged(sender, e);
        shootAudioSource.volume = sfxVolume * volumeMultiplier;
    }
}
