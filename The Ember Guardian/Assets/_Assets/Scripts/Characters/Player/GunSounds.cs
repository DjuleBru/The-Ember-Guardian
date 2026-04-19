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

    [SerializeField] private float surgeMaxDuration = 10f;

    private float surgeTimer = 0f;
    private bool surgeTimerRunning = false;

    protected Gun gun;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();
        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
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

        PreloadAudioClips();
    }
    protected virtual void Update() {

        if (surgeTimerRunning == false) {
            return;
        }

         surgeTimer += Time.deltaTime;

         if (surgeTimer >= surgeMaxDuration) {
            StopSurge();
        }
    }

    protected virtual void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGunSO() != gun.GetGunSO()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;

        if(gun.GetCurrentBullet() != 0) {

            bulletAudioSource.pitch = 1f;
            AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGun().GetComponent<GunSounds>().GetShootAudioClips();
            float volume = PlayerShoot.Instance.GetHeldGunSO().shootGunVolumeMultiplier * sfxVolume * masterVolume * 2;
            AudioClip audioClip = audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)];

            bulletAudioSource.PlayOneShot(audioClip, volume);
        } else {

            PlayLastBulletEffect();

        }

    }
    protected void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        if (gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            StartSurge();
        }
    }

    protected virtual void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
            surgeAudioSource.Stop();
            surgeTimerRunning = false;
        }
    }

    protected void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if(gun.GetGunActive() && gun.GetDamageSurgeBuffed() && PlayerShoot.Instance.GetCurrentBullets() > 0) {
            StartSurge();
        }

        if (!gun.GetGunActive() && gun.GetDamageSurgeBuffed()) {
            surgeAudioSource.Stop();
            surgeTimerRunning = false;
        }
    }

    protected void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        StopSurge();
    }

    protected void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        StartSurge();
    }

    public virtual AudioClip[] GetShootAudioClips() {
        return gun.GetGunSO().shootGunSound;
    }

    protected void PlayLastBulletEffect() {
        if (bulletAudioSource == null || gun == null) return;

        // Modifie pour la dernière balle
        bulletAudioSource.pitch = lastBulletPitch;
        float volume = lastBulletVolume * gun.GetGunSO().shootGunVolumeMultiplier * sfxVolume * masterVolume;

        // Joue le même clip de tir
        var clips = GetShootAudioClips();
        if (clips.Length > 0) {
            bulletAudioSource.PlayOneShot(clips[0], volume);
        }
    }

    private void StartSurge() {

        if (surgeAudioSource == null) {
            return;
        }

        surgeAudioSource.Play();

        surgeTimer = 0f;
        surgeTimerRunning = true;
    }

    private void StopSurge() {

        if (surgeAudioSource == null) {
            return;
        }

        surgeAudioSource.Stop();

        surgeTimerRunning = false;
        surgeTimer = 0f;
    }

    private void PreloadAudioClips() {
        if(PlayerShoot.Instance.GetPrimaryGunSO() == gun.GetGunSO() || PlayerShoot.Instance.GetSecondaryGunSO() == gun.GetGunSO()) {
            if (gun != null && gun.GetGunSO() != null) {

                AudioClip[] shootClips = gun.GetGunSO().shootGunSound;

                if (shootClips != null) {

                    for (int i = 0; i < shootClips.Length; i++) {

                        AudioClip clip = shootClips[i];

                        if (clip == null) continue;

                        if (clip.loadState == AudioDataLoadState.Unloaded) {
                            clip.LoadAudioData();
                        }
                    }
                }
            }
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
