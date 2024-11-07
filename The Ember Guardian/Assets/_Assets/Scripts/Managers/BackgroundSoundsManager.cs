using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSoundsManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;

    [SerializeField] private AudioClip dayAudioClip;
    [SerializeField] private AudioClip nightAudioClip;

    public float transitionDuration = 2.0f; // Durée de la transition en secondes

    private bool isTransitioning = false;

    private void Start() {
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        // Assure-toi que les deux sources sont au volume initial
        audioSource1.volume = 1.0f;
        audioSource2.volume = 0.0f;

        audioSource1.clip = dayAudioClip;
        audioSource1.Play();
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        TransitionToClip(nightAudioClip);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        TransitionToClip(dayAudioClip);
    }

    public void TransitionToClip(AudioClip newClip) {
        if (isTransitioning) return; // Évite de lancer une nouvelle transition si une est en cours

        AudioSource activeSource = audioSource1.isPlaying ? audioSource1 : audioSource2;
        AudioSource nextSource = audioSource1.isPlaying ? audioSource2 : audioSource1;

        // Configure le nouveau clip sur la source inactive
        nextSource.clip = newClip;
        nextSource.Play();

        // Démarre la transition
        StartCoroutine(Crossfade(activeSource, nextSource));
    }

    private IEnumerator Crossfade(AudioSource fromSource, AudioSource toSource) {
        isTransitioning = true;
        float timeElapsed = 0.0f;

        while (timeElapsed < transitionDuration) {
            float t = timeElapsed / transitionDuration;
            fromSource.volume = Mathf.Lerp(1.0f, 0.0f, t);
            toSource.volume = Mathf.Lerp(0.0f, 1.0f, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Assure-toi que les volumes sont bien ajustés
        fromSource.volume = 0.0f;
        toSource.volume = 1.0f;

        fromSource.Stop(); // Arrête l'audio source précédente
        isTransitioning = false;
    }
}
