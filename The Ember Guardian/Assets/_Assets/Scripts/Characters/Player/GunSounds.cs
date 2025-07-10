using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds : SoundObject
{
    [SerializeField] private AudioClip surgeAudioClip;
    [SerializeField] private AudioSource surgeAudioSource;
    [SerializeField] private float surgeBuffVolume;

    private Gun gun;

    protected void Awake() {
        gun = GetComponent<Gun>();
        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
    }

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;

        surgeAudioSource.loop = true;
        surgeAudioSource.clip = surgeAudioClip;
        surgeAudioSource.volume = sfxVolume * surgeBuffVolume;
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if(gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            surgeAudioSource.Play();
        }

        if (!gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            surgeAudioSource.Stop();
        }
    }

    private void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        surgeAudioSource.Stop();
    }

    private void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        surgeAudioSource.Play();
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        surgeAudioSource.volume = sfxVolume * surgeBuffVolume;
    }
}
