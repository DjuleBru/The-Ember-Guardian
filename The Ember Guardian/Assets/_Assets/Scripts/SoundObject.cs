using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    protected float sfxVolume;
    protected AudioSource audioSource2D;

    protected virtual void Start() {
        audioSource2D = GetComponent<AudioSource>();
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
    }


    protected virtual void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    public void FadeOut(AudioSource audioSource, float fadeDuration) {
        StartCoroutine(FadeOutCoroutine(audioSource, fadeDuration));
    }

    public void FadeIn(AudioSource audioSource, float fadeDuration, float targetVolume) {
        StartCoroutine(FadeInCoroutine(audioSource, fadeDuration, targetVolume));
    }


    protected void PlaySound2D(AudioClip[] audioClipArray, float volume = 1f) {
        if (audioClipArray.Length == 0) return;
        AudioClip audioClip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        PlaySound2D(audioClip, volume);
    }

    protected void PlaySound2D(AudioClip audioClip, float volume = 1f) {
        audioSource2D.PlayOneShot(audioClip, volume * sfxVolume);
    }

    protected void PlaySFXAfterDelay(AudioClip audioClip, float delay, float volume = 1f) {
        StartCoroutine(PlaySFXAfterDelayCoroutine(audioClip, delay, volume));
    }
    protected void PlaySFXAfterDelay(AudioClip[] audioClip, float delay, float volume = 1f) {
        StartCoroutine(PlaySFXAfterDelayCoroutine(audioClip, delay, volume));
    }

    private IEnumerator PlaySFXAfterDelayCoroutine(AudioClip audioClip,float delay, float volume = 1f) {
        yield return new WaitForSeconds(delay);
        PlaySound2D(audioClip, volume);
    }
    private IEnumerator PlaySFXAfterDelayCoroutine(AudioClip[] audioClip, float delay, float volume = 1f) {
        yield return new WaitForSeconds(delay);
        PlaySound2D(audioClip, volume);
    }

    protected IEnumerator FadeOutCoroutine(AudioSource audioSource, float fadeDuration) {
        float startVolume = audioSource.volume;

        // Réduire progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique pour le ressenti
            audioSource.volume = Mathf.Lerp(startVolume, 0, Mathf.Sqrt(progress));
            yield return null;
        }

        // S'assurer que le volume est bien à 0 à la fin
        audioSource.volume = 0;
        audioSource.Stop(); // Arrêter la musique
    }

    protected IEnumerator FadeInCoroutine(AudioSource audioSource, float fadeDuration, float targetVolume) {
        audioSource.volume = 0;
        audioSource.Play(); // Assure que la musique démarre

        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique inverse pour le ressenti
            audioSource.volume = Mathf.Lerp(0, targetVolume, progress * progress);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume atteint la valeur finale
        audioSource.volume = targetVolume;
    }

    protected IEnumerator ChangeVolumeGradually(float targetVolume, bool stopAfter = false)
    {
        float duration = 1.0f; // Temps de transition
        float elapsed = 0f;
        float startVolume = audioSource2D.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource2D.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource2D.volume = targetVolume;
        
        if(targetVolume == 0)
        {
            stopAfter = true;   
        }
        if (stopAfter)
        {
            audioSource2D.Stop();
        }
    }
}
