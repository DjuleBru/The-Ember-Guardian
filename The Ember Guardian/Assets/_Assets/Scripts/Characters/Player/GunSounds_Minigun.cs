using UnityEngine;
using System.Collections;

public class GunSounds_Minigun : GunSounds {
    [SerializeField] private AudioClip gunPoweringUpClip;
    [SerializeField] private AudioClip gunPoweringDownClip;
    [SerializeField] private AudioClip gunPoweredClip;
    [SerializeField] protected AudioSource poweringAudioSource;
    [SerializeField] private float powerUpDelay = 0.5f; // Délai avant de commencer le powering up
    [SerializeField] private float powerSourceVolumeMultiplier = 1f;

    private Coroutine currentSoundRoutine;
    private bool isShooting = false;
    private bool isLoopingPowered = false;

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;


        poweringAudioSource.volume = sfxVolume * powerSourceVolumeMultiplier;
    }

    protected override void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        base.PlayerShoot_OnPlayerShot(sender, e);

        if (!isShooting) {
            isShooting = true;

            if (currentSoundRoutine != null)
                StopCoroutine(currentSoundRoutine);

            currentSoundRoutine = StartCoroutine(DelayedPowerUpAndLoop());
        }
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        if (!isShooting) return;
        isShooting = false;

        if (currentSoundRoutine != null) {
            StopCoroutine(currentSoundRoutine);
            currentSoundRoutine = null;
        }

        if (isLoopingPowered) {
            // On était en train de loop le minigun => on joue powering down
            isLoopingPowered = false;
            poweringAudioSource.Stop();
            poweringAudioSource.loop = false;
            poweringAudioSource.clip = gunPoweringDownClip;
            poweringAudioSource.Play();
        }
        else {
            // Sinon on coupe simplement le son (powering up ou attente)
            poweringAudioSource.Stop();
        }
    }

    private IEnumerator DelayedPowerUpAndLoop() {
        // Phase attente avant de commencer le powering up
        yield return new WaitForSeconds(powerUpDelay);

        if (!isShooting)
            yield break;

        // Powering Up
        poweringAudioSource.Stop();
        poweringAudioSource.loop = false;
        poweringAudioSource.clip = gunPoweringUpClip;
        poweringAudioSource.Play();

        yield return new WaitForSeconds(gunPoweringUpClip.length);

        if (!isShooting)
            yield break;

        // Boucle sur gunPoweredClip
        isLoopingPowered = true;
        poweringAudioSource.clip = gunPoweredClip;
        poweringAudioSource.loop = true;
        poweringAudioSource.Play();
    }
    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        base.SettingsManager_OnSfxVolumeChanged (sender, e);

        poweringAudioSource.volume = sfxVolume * powerSourceVolumeMultiplier;
    }
}
