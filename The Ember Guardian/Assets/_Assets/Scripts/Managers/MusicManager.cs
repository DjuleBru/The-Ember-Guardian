using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private float audioVolume = .2f;
    [SerializeField] private AudioClip endLevelMusic;


    private float hubDelayToStartPlayingMusic = 2f;
    private AudioSource audioSource;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        SetAudioVolume(audioVolume);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
            audioSource.PlayDelayed(hubDelayToStartPlayingMusic);
        }
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        FadeOutMusic(1f);
    }

    public void FadeOutMusic(float fadeDuration) {
        StartCoroutine(FadeOutCoroutine(fadeDuration));
    }
    private IEnumerator FadeOutCoroutine(float fadeDuration) {
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

    private IEnumerator FadeInCoroutine(float fadeDuration) {
        float endVolume = audioVolume;
        audioSource.volume = 0;
        audioSource.Play(); // Assure que la musique démarre

        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique inverse pour le ressenti
            audioSource.volume = Mathf.Lerp(0, endVolume, progress * progress);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume atteint la valeur finale
        audioSource.volume = endVolume;
    }
    public void SetAudioVolume(float volume) {
        audioSource.volume = volume;
    }

    public void SetAudioTargerVolume(float volume) {
        audioVolume = volume;
    }

    public void FadeInMusic(float fadeDuration) {
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeInCoroutine(fadeDuration));
    }

    public void SetEndLevelMusic() {
        audioSource.clip = endLevelMusic;
    }


}
