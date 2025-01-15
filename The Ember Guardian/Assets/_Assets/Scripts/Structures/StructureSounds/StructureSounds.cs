using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSounds : MonoBehaviour
{
    protected float sfxVolume;
    protected AudioSource audioSource;

    protected virtual void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        SceneLoader.Instance.OnSceneFadeOut += SceneLoader_OnSceneFadeOut;
    }

    private void SceneLoader_OnSceneFadeOut(object sender, System.EventArgs e) {
        StartCoroutine(FadeOutCoroutine(1f));
    }

    protected virtual void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
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
}
