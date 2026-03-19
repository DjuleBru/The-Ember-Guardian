using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds_GrenadeLauncher : GunSounds
{
    [SerializeField] private AudioClip[] shootMultipleGrenadesAudioClips;
    protected override void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGunSO() != gun.GetGunSO()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;

        if (PlayerShoot.Instance.GetGrenadeLauncherMultipleGrenadesActive()) {

            AudioClip clip = shootMultipleGrenadesAudioClips[UnityEngine.Random.Range(0, shootMultipleGrenadesAudioClips.Length)];
            bulletAudioSource.PlayOneShot(clip);

        }
        else {

            bulletAudioSource.pitch = 1f;
            AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGun().GetComponent<GunSounds>().GetShootAudioClips();
            float volume = PlayerShoot.Instance.GetHeldGunSO().shootGunVolumeMultiplier * sfxVolume * masterVolume;

            AudioClip audioClip = audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)];

            bulletAudioSource.PlayOneShot(audioClip, volume);
        }

    }

    protected override void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if(PlayerShoot.Instance.GetGrenadeLauncherMultipleGrenadesActive()) {

        } else {
            base.PlayerShoot_OnPlayerStartedShot(sender, e);
        }
    }
}
