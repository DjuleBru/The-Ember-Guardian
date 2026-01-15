using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds : SoundObject
{
    [SerializeField] protected AudioClip surgeAudioClip;
    [SerializeField] protected AudioSource surgeAudioSource;
    [SerializeField] protected AudioSource bulletAudioSource;
    [SerializeField] protected float surgeBuffVolume;

    [SerializeField] protected float lastBulletPitch = 1.3f;  // Pitch pour la dernière balle
    [SerializeField] protected float lastBulletVolume = 1.2f; // Volume pour la dernière balle

    protected Gun gun;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();
        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
    }

    private void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGunSO() != gun.GetGunSO()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;

        if(gun.GetCurrentBullet() != 0) {
            bulletAudioSource.pitch = 1f;
            AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGun().GetComponent<GunSounds>().GetShootAudioClips();
            float volume = PlayerShoot.Instance.GetHeldGunSO().shootGunVolumeMultiplier * sfxVolume;
            AudioClip audioClip = audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)];

            bulletAudioSource.PlayOneShot(audioClip, volume);

        } else {

            PlayLastBulletEffect();

        }

    }

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerStartedShot += PlayerShoot_OnPlayerStartedShot;

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
    private void PlayLastBulletEffect() {
        if (bulletAudioSource == null || gun == null) return;

        // Modifie pour la dernière balle
        bulletAudioSource.pitch = lastBulletPitch;
        float volume = lastBulletVolume * gun.GetGunSO().shootGunVolumeMultiplier * sfxVolume;

        // Joue le même clip de tir
        var clips = GetShootAudioClips();
        if (clips.Length > 0) {
            bulletAudioSource.PlayOneShot(clips[0], volume);
        }
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
