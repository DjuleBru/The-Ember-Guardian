using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private float audioVolume = .2f;

    private float hubDelayToStartPlayingMusic = 2f;
    private AudioSource audioSource;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        SetAudioVolume();

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
            audioSource.PlayDelayed(hubDelayToStartPlayingMusic);
        }
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        FadeOutMusic();
    }

    public void FadeOutMusic() {
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

    private IEnumerator FadeInCoroutine(float fadeDuration) {
        float endVolume = audioVolume;

        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            audioSource.volume = Mathf.Lerp(0, endVolume, t / fadeDuration);
            yield return null; // Attendre le prochain frame
        }
    }
    private void SetAudioVolume() {
        audioSource.volume = audioVolume;
    }

    public void FadeInMusic(float fadeDuration) {
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeInCoroutine(fadeDuration));
    }
}
