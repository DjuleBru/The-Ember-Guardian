using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds : SoundObject
{
    [SerializeField] protected AudioClip surgeAudioClip;
    [SerializeField] protected AudioSource surgeAudioSource;
    [SerializeField] protected float surgeBuffVolume;

    protected Gun gun;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();
        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
    }

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;

        surgeAudioSource.loop = true;
        surgeAudioSource.clip = surgeAudioClip;
        surgeAudioSource.volume = sfxVolume * surgeBuffVolume;
    }


    protected void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        if (gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            surgeAudioSource.Play();
        }
    }

    protected virtual void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
            surgeAudioSource.Stop();
        }
    }

    protected void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if(gun.GetGunActive() && gun.GetDamageSurgeBuffed() && PlayerShoot.Instance.GetCurrentBullets() > 0) {
            surgeAudioSource.Play();
        }

        if (!gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            surgeAudioSource.Stop();
        }
    }

    protected void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        surgeAudioSource.Stop();
    }

    protected void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        surgeAudioSource.Play();
    }

    public virtual AudioClip[] GetShootAudioClips() {
        return gun.GetGunSO().shootGunSound;
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        surgeAudioSource.volume = sfxVolume * masterVolume * surgeBuffVolume;
    }
    protected override void SettingsManager_OnMasterVolumeChanged(object sender, System.EventArgs e) {
        masterVolume = SettingsManager.Instance.GetMasterVolume();
        surgeAudioSource.volume = sfxVolume * masterVolume * surgeBuffVolume;
    }
}
