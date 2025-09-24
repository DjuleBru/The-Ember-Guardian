using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnerContinuous_Sound : MonoBehaviour {

    [SerializeField] private CreatureSpawnerContinuous creatureSpawner;
    [SerializeField] private float summonSFXVolumeMultiplier = 1f;

    [SerializeField] private AudioClip[] spawnerDiedAudioClips;
    [SerializeField] private AudioClip[] spawnerSpawnerAudioClips;
    [SerializeField] private AudioClip[] spawnerDamagedAudioClips;

    private AudioSource spawnerAudioSource;

    private float sfxVolume;
    private float masterVolume;

    private void Awake() {
        spawnerAudioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        masterVolume = SettingsManager.Instance.GetMasterVolume();

        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        SettingsManager.Instance.OnMasterVolumeChanged += SettingsManager_OnMasterVolumeChanged;
        creatureSpawner.OnSpawnerSpawnStart += CreatureSpawner_OnSpawnerSpawnStart;
        creatureSpawner.OnSpawnerDied += CreatureSpawner_OnSpawnerDied;
        creatureSpawner.OnSpawnerDamaged += CreatureSpawner_OnSpawnerDamaged;
    }


    private void CreatureSpawner_OnSpawnerDamaged(object sender, System.EventArgs e) {
        //spawnerAudioSource.PlayOneShot(spawnerDamagedAudioClips[Random.Range(0, spawnerDamagedAudioClips.Length)], sfxVolume);
    }

    private void CreatureSpawner_OnSpawnerDied(object sender, System.EventArgs e) {
        spawnerAudioSource.PlayOneShot(spawnerDiedAudioClips[Random.Range(0, spawnerDiedAudioClips.Length)], sfxVolume * masterVolume);

    }

    private void CreatureSpawner_OnSpawnerSpawnStart(object sender, System.EventArgs e) {
        spawnerAudioSource.PlayOneShot(spawnerSpawnerAudioClips[Random.Range(0, spawnerSpawnerAudioClips.Length)], summonSFXVolumeMultiplier * sfxVolume * masterVolume);

    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }
    private void SettingsManager_OnMasterVolumeChanged(object sender, System.EventArgs e) {
        masterVolume = SettingsManager.Instance.GetMasterVolume();
    }
}
