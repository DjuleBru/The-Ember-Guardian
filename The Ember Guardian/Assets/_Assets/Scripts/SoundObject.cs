using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    protected float masterVolume;
    protected float sfxVolume;
    protected AudioSource audioSource2D;

    private static Dictionary<AudioClip, float> lastPlayedTime = new Dictionary<AudioClip, float>();
    private static Dictionary<AudioClip, int> clipInstances = new Dictionary<AudioClip, int>();
    [SerializeField] private float minIntervalBetweenSameClip = 0.1f; // délai minimal entre deux sons identiques
    [SerializeField] private int maxInstancesPerClip = 10;             // nombre max global d’instances par son

    protected virtual void Start() {
        audioSource2D = GetComponent<AudioSource>();
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        masterVolume = SettingsManager.Instance.GetMasterVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        SettingsManager.Instance.OnMasterVolumeChanged += SettingsManager_OnMasterVolumeChanged;
    }

    protected virtual void SettingsManager_OnMasterVolumeChanged(object sender, System.EventArgs e) {
        masterVolume = SettingsManager.Instance.GetMasterVolume();
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
        if (audioSource2D.volume == 0) return;

        if (audioClipArray.Length == 1) {
            float originalPitch = audioSource2D.pitch;
            audioSource2D.pitch = Random.Range(0.95f, 1.05f); // Pitch légèrement aléatoire
            PlaySound2D(audioClipArray[0], volume);
            audioSource2D.pitch = originalPitch; // On remet le pitch à la normale
        }
        else {
            AudioClip audioClip = audioClipArray[Random.Range(0, audioClipArray.Length)];
            PlaySound2D(audioClip, volume);
        }

    }

    protected void PlaySound2D(AudioClip audioClip, float volume = 1f) {
        if (audioClip == null) return;

        // Vérif cooldown global
        float lastTime;
        if (lastPlayedTime.TryGetValue(audioClip, out lastTime)) {
            if (Time.time - lastTime < minIntervalBetweenSameClip) {
                return; // trop tôt : on ignore
            }
        }

        // Vérif instances simultanées globales
        int count;
        clipInstances.TryGetValue(audioClip, out count);
        if (count >= maxInstancesPerClip) {
            return; // déjà trop d’instances actives
        }

        // Mise à jour des registres
        lastPlayedTime[audioClip] = Time.time;
        if (!clipInstances.ContainsKey(audioClip)) {
            clipInstances[audioClip] = 0;
        }
        clipInstances[audioClip]++;

        // Lecture et décrément après la durée du son
        audioSource2D.PlayOneShot(audioClip, volume * sfxVolume * masterVolume);
        StartCoroutine(TrackClipInstance(audioClip, audioClip.length));
    }

    private IEnumerator TrackClipInstance(AudioClip clip, float duration) {
        yield return new WaitForSeconds(duration);
        clipInstances[clip]--;
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
