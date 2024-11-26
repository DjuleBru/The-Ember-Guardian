using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVolume2D : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float maxDistanceToHear = 20f;
    [SerializeField] private float maxAudioSourceVolume = 1f;

    private float sfxVolume;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void Update() {
        float distanceToAudioSource = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        float volume = (1 - (distanceToAudioSource / maxDistanceToHear)) * maxAudioSourceVolume * sfxVolume;

        if(volume < 0) volume = 0;

        audioSource.volume = volume;
    }

    public void SetMaxDistanceToHear(float distance) {
        maxDistanceToHear = distance;
    }

}
