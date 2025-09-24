using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSoundsManager : MonoBehaviour
{
    public static BackgroundSoundsManager Instance;

    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;
    [SerializeField] private AudioSource cycleTransitionSounds;
    [SerializeField] private AudioSource cycleTransitionWhoosh;

    [SerializeField] private AudioClip dayAudioClip;
    [SerializeField] private AudioClip nightAudioClip;

    [SerializeField] private SoundRefsSO soundRefs;

    [SerializeField] private float audioClipVolume_Day;
    [SerializeField] private float audioClipVolume_Night;

    public float transitionDuration = 2.0f; // Durée de la transition en secondes

    private bool isTransitioning = false;
    private bool isInCavern = false;
    private bool initialStateSet;
    private AudioSource currentAudioSourcePlaying;

    private float sfxVolume;
    private float masterVolume;

    private void Awake() {
        Instance = this;
    }


    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        masterVolume = SettingsManager.Instance.GetMasterVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        SettingsManager.Instance.OnMasterVolumeChanged += SettingsManager_OnMasterVolumeChanged;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        // Assure-toi que les deux sources sont au volume initial
        audioSource1.volume = audioClipVolume_Day * sfxVolume;
        audioSource2.volume = 0.0f;

        audioSource1.clip = dayAudioClip;
        audioSource1.Play();
        currentAudioSourcePlaying = audioSource1;
    }

    private void SettingsManager_OnMasterVolumeChanged(object sender, System.EventArgs e) {
        masterVolume = SettingsManager.Instance.GetMasterVolume();

        float newVolume = masterVolume * sfxVolume * audioClipVolume_Day;
        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            newVolume = masterVolume * sfxVolume * audioClipVolume_Night;
        }
        currentAudioSourcePlaying.volume = newVolume;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();

        float newVolume = masterVolume * sfxVolume * audioClipVolume_Day;
        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            newVolume = masterVolume * sfxVolume * audioClipVolume_Night;
        }
        currentAudioSourcePlaying.volume = newVolume;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if(!initialStateSet) {
            initialStateSet = true;
            return;
        }

        cycleTransitionWhoosh.PlayOneShot(soundRefs.duskStartWhoosh);
        cycleTransitionSounds.PlayOneShot(soundRefs.duskStart);

        StartCoroutine(FadeInThenOutCoroutine(cycleTransitionSounds, 1f, .3f, 2f));
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (!initialStateSet) {
            initialStateSet = true;
            return;
        }
        cycleTransitionWhoosh.PlayOneShot(soundRefs.dayStartWhoosh);
        cycleTransitionSounds.PlayOneShot(soundRefs.dayStart);

        StartCoroutine(FadeInThenOutCoroutine(cycleTransitionSounds, .5f, .3f, 2f));
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        if (!initialStateSet) {
            initialStateSet = true;
            return;
        }

        cycleTransitionWhoosh.PlayOneShot(soundRefs.nightStartWhoosh);
        cycleTransitionSounds.PlayOneShot(soundRefs.nightStart);
        StartCoroutine(FadeInThenOutCoroutine(cycleTransitionSounds, .5f, .3f, 5f));

        if (isInCavern) return;
        TransitionToClip(nightAudioClip, audioClipVolume_Night);

    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (!initialStateSet) {
            initialStateSet = true;
            return;
        }

        cycleTransitionWhoosh.PlayOneShot(soundRefs.dawnStartWhoosh);
        cycleTransitionSounds.PlayOneShot(soundRefs.dawnStart);
        StartCoroutine(FadeInThenOutCoroutine(cycleTransitionSounds, .5f, .3f, 2f));

        if (isInCavern) return;
        TransitionToClip(dayAudioClip, audioClipVolume_Day);
    }

    public void TransitionToClip(AudioClip newClip, float volumeToReach) {
        if (isTransitioning) return; // Évite de lancer une nouvelle transition si une est en cours

        AudioSource activeSource = audioSource1.isPlaying ? audioSource1 : audioSource2;
        AudioSource nextSource = audioSource1.isPlaying ? audioSource2 : audioSource1;

        // Configure le nouveau clip sur la source inactive
        nextSource.clip = newClip;
        nextSource.Play();

        currentAudioSourcePlaying = nextSource;

        // Démarre la transition
        StartCoroutine(Crossfade(activeSource, nextSource, volumeToReach));
    }

    public void SetInCavern(bool inCavern, AudioClip cavernBackgroundAudioClip, float cavernBackgroundVolume) { 
        isInCavern = inCavern;

        if(isInCavern) {
            TransitionToClip(cavernBackgroundAudioClip, cavernBackgroundVolume);
        } else {
            if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                TransitionToClip(nightAudioClip, audioClipVolume_Night);
            } else {
                TransitionToClip(dayAudioClip, audioClipVolume_Day);
            }
        }
    }

    private IEnumerator Crossfade(AudioSource fromSource, AudioSource toSource, float volumeToReach) {
        isTransitioning = true;
        float timeElapsed = 0.0f;

        while (timeElapsed < transitionDuration) {
            float t = timeElapsed / transitionDuration;
            fromSource.volume = Mathf.Lerp(volumeToReach * sfxVolume, 0.0f, t);
            toSource.volume = Mathf.Lerp(0.0f, volumeToReach * sfxVolume, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Assure-toi que les volumes sont bien ajustés
        fromSource.volume = 0.0f;
        toSource.volume = volumeToReach;

        fromSource.Stop(); // Arrête l'audio source précédente
        isTransitioning = false;
    }

    private IEnumerator FadeInThenOutCoroutine(AudioSource audioSource, float fadeDuration, float volumeToReach, float timeBetweenFades) {

        StartCoroutine(FadeAudioCoroutine(audioSource, fadeDuration, volumeToReach, true));

        yield return new WaitForSeconds(timeBetweenFades + fadeDuration);

        StartCoroutine(FadeAudioCoroutine(audioSource, fadeDuration, volumeToReach, false));

    }

    private IEnumerator FadeAudioCoroutine(AudioSource audioSource, float fadeDuration, float volumeToReach, bool fadeIn) {
        float startVolume;
        float endVolume;

        if (fadeIn) {
            startVolume = 0;
            endVolume = volumeToReach * masterVolume * sfxVolume;
            audioSource.Play();
        }
        else {
            startVolume = audioSource.volume;
            endVolume = 0;
        }

        float startTime = Time.time;

        while (Time.time < startTime + fadeDuration) {
            // Calcule la progression du fade (0 à 1)
            float t = (Time.time - startTime) / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return null;
        }

        // Assure la valeur finale du volume
        audioSource.volume = endVolume;

        if (!fadeIn) {
            audioSource.Stop();
        }
    }

}
