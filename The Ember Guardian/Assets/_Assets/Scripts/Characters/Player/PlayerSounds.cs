using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource playerAudioSource;

    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private AudioClip[] playerDamagedAudioClips;
    [SerializeField] private AudioClip[] playerDiedAudioClips;
    [SerializeField] private AudioClip[] playerDamagedElectricAudioClips;
    [SerializeField] private AudioClip activeMoveSpeedBoostFootstepAudioClip;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private AnimationCurve pitchWithSpeedAnimationCurve;
    [SerializeField] private ActiveMoveSpeedBoostVisual activeMoveSpeedBoostVisual;

    private float sfxVolume;

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;

        playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        activeMoveSpeedBoostVisual.OnMoveSpeedFootStepTriggered += ActiveMoveSpeedBoostVisual_OnMoveSpeedFootStepTriggered;
    }


    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDiedAudioClips[Random.Range(0, playerDiedAudioClips.Length)], sfxVolume);
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDamagedElectricAudioClips[Random.Range(0, playerDamagedElectricAudioClips.Length)], sfxVolume);

        if (Player.Instance.GetHP() == 0) return;
        StartCoroutine(PlayHumanDamagedAudioClip());
    }

    private IEnumerator PlayHumanDamagedAudioClip() {
        yield return new WaitForSeconds(.075f);
        playerAudioSource.PlayOneShot(playerDamagedAudioClips[Random.Range(0, playerDamagedAudioClips.Length)], sfxVolume);
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(footStepAudioClips[Random.Range(0, footStepAudioClips.Length)], sfxVolume);
    }

    private void ActiveMoveSpeedBoostVisual_OnMoveSpeedFootStepTriggered(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(activeMoveSpeedBoostFootstepAudioClip, sfxVolume / 8);
    }
}
