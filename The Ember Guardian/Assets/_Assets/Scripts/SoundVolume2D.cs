using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVolume2D : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float maxDistanceToHear = 20f;
    [SerializeField] private float maxAudioSourceVolume = 1f;

    [SerializeField] private bool active = true;
    private bool isPlaying = true;
    private float sfxVolume;
    private float fadeMultiplier = 1f;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        if(!active) {
            audioSource.enabled = false;
        }
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        audioSource.volume = sfxVolume;
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void Update() {
        if (!active) return;

        float distanceToAudioSource = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        float volume = (1 - (distanceToAudioSource / maxDistanceToHear)) * maxAudioSourceVolume * sfxVolume;

        if (volume < 0 && isPlaying) {
            audioSource.Stop();
            isPlaying = false;
            volume = 0;
            return;
        } 

        if(volume > 0 && !isPlaying) {
            audioSource.Play();
            isPlaying = true;
        }

        audioSource.volume = volume * fadeMultiplier;
    }

    public void SetMaxDistanceToHear(float distance) {
        maxDistanceToHear = distance;
    }
    public void SetFadeMultiplier(float value) {
        fadeMultiplier = Mathf.Clamp01(value);
    }
}
