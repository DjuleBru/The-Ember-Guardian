using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private float delayToStartPlayingMusic = 2f;
    private float audioVolume = .2f;
    private AudioSource audioSource;

    private void Start() {
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        audioSource = GetComponent<AudioSource>();
        SetAudioVolume();
        audioSource.PlayDelayed(delayToStartPlayingMusic);
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        FadeOutMusic();
    }

    private void FadeOutMusic() {
        StartCoroutine(FadeOutCoroutine());
    }
    private IEnumerator FadeOutCoroutine() {
        float fadeDuration = 1f; // Durée du fade-out en secondes
        float startVolume = audioSource.volume;

        // Réduire progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume est bien à 0 à la fin
        audioSource.volume = 0;
        audioSource.Stop(); // Arrêter la musique
    }

    private void SetAudioVolume() {
        audioSource.volume = audioVolume;
    }
}
