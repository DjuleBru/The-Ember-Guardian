using UnityEngine;
using System.Collections;

public class GunSounds_Minigun : GunSounds {
    [SerializeField] private AudioClip gunPoweringDownClip;
    [SerializeField] private AudioClip gunPoweredClip;
    [SerializeField] protected AudioSource poweringAudioSource;
    [SerializeField] private float powerUpDelay = 0.5f; // Délai avant de commencer le powering up
    [SerializeField] private float powerSourceVolumeMultiplier = 1f;
    [SerializeField] private Gun_Minigun minigun;

    [SerializeField] private AudioClip gunPoweringUpClip;
    //[SerializeField] private AudioClip gunPoweringUpClip_3_5s;
    //[SerializeField] private AudioClip gunPoweringUpClip_3s;
    //[SerializeField] private AudioClip gunPoweringUpClip_2_5s;
    //[SerializeField] private AudioClip gunPoweringUpClip_2s;

    private Coroutine currentSoundRoutine;
    private bool isSpinning = false;
    private bool isLoopingPowered = false;

    protected override void Start() {
        base.Start();
        minigun.OnMinigunStartedSpinning += Minigun_OnMinigunStartedSpinning;
        minigun.OnMinigunStoppedSpinning += Minigun_OnMinigunStoppedSpinning;

        poweringAudioSource.volume = sfxVolume * powerSourceVolumeMultiplier;
    }

    private void Minigun_OnMinigunStoppedSpinning(object sender, System.EventArgs e) {
        if (!isSpinning) return;
        isSpinning = false;

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

    private void Minigun_OnMinigunStartedSpinning(object sender, System.EventArgs e) {
        SetCorrectPoweringUpClip();
        if (!isSpinning) {
            isSpinning = true;

            if (currentSoundRoutine != null)
                StopCoroutine(currentSoundRoutine);

            currentSoundRoutine = StartCoroutine(DelayedPowerUpAndLoop());
        }
    }

    private void SetCorrectPoweringUpClip() {
    }

    private IEnumerator DelayedPowerUpAndLoop() {
        yield return new WaitForSeconds(powerUpDelay);

        if (!isSpinning)
            yield break;

        float spinUpDuration = minigun.GetSpinUpDuration();
        float clipDuration = gunPoweringUpClip.length;

        poweringAudioSource.Stop();
        poweringAudioSource.loop = false;
        poweringAudioSource.clip = gunPoweringUpClip;

        // Calcule le pitch pour caler la durée du clip sur le spinUp
        float pitch = clipDuration / spinUpDuration;
        poweringAudioSource.pitch = pitch;
        poweringAudioSource.Play();

        yield return new WaitForSeconds(spinUpDuration);

        if (!isSpinning)
            yield break;

        // Boucle sur gunPoweredClip
        isLoopingPowered = true;
        poweringAudioSource.clip = gunPoweredClip;
        poweringAudioSource.loop = true;
        poweringAudioSource.Play();

        // On garde le pitch actuel au début, puis on le ramène progressivement à 1
        StartCoroutine(SmoothPitchReset(poweringAudioSource, poweringAudioSource.pitch, 1f, 0.3f));
    }
    private IEnumerator SmoothPitchReset(AudioSource source, float startPitch, float targetPitch, float fadeTime) {
        float elapsed = 0f;

        while (elapsed < fadeTime) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            source.pitch = Mathf.Lerp(startPitch, targetPitch, t);
            yield return null;
        }

        source.pitch = targetPitch; // sécurité pour terminer pile à 1
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        base.SettingsManager_OnSfxVolumeChanged (sender, e);

        poweringAudioSource.volume = sfxVolume * powerSourceVolumeMultiplier;
    }

}
