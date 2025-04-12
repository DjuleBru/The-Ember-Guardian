using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : SoundObject
{
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource playerReloadAudioSource;

    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private AudioClip[] playerDamagedAudioClips;
    [SerializeField] private AudioClip[] playerDiedAudioClips;
    [SerializeField] private AudioClip[] playerDamagedElectricAudioClips;
    [SerializeField] private AudioClip[] playerPantAudioClips;
    [SerializeField] private AudioClip[] playerExhaustedAudioClips;
    [SerializeField] private AudioClip[] playerRollAudioClips;
    [SerializeField] private AudioClip[] playerMeleeAttackStartedAudioClips;
    [SerializeField] private AudioClip[] playerMeleeAttackHitAudioClips;
    [SerializeField] private AudioClip activeMoveSpeedBoostFootstepAudioClip;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerBreathAnimator playerBreathAnimator;
    [SerializeField] private AnimationCurve pitchWithSpeedAnimationCurve;
    [SerializeField] private ActiveMoveSpeedBoostVisual activeMoveSpeedBoostVisual;

    private bool almostExhausted;
    private bool exhaustedSFXPlaying;
    protected override void Start() {
        base.Start();

        playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
        playerBreathAnimator.OnPantTriggered += PlayerAnimator_OnPantTriggered;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionDeactivateFeedbacks += PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks;

        PlayerMeleeAttack.Instance.OnMeleeAttackStarted += PlayerMeleeAttack_OnMeleeAttackStarted;
        GunMeleeAttackCollider.OnAnyGunMeleeAttackHit += GunMeleeAttackCollider_OnAnyGunMeleeAttackHit;

        activeMoveSpeedBoostVisual.OnMoveSpeedFootStepTriggered += ActiveMoveSpeedBoostVisual_OnMoveSpeedFootStepTriggered;
    }

    private void GunMeleeAttackCollider_OnAnyGunMeleeAttackHit(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerMeleeAttackHitAudioClips[Random.Range(0, playerMeleeAttackHitAudioClips.Length)], sfxVolume*.7f);
    }

    private void PlayerMeleeAttack_OnMeleeAttackStarted(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerMeleeAttackStartedAudioClips[Random.Range(0, playerMeleeAttackStartedAudioClips.Length)], sfxVolume);
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks(object sender, System.EventArgs e) {
        almostExhausted = false;
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStarted(object sender, System.EventArgs e) {
        almostExhausted = true;
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, System.EventArgs e) {
        playerReloadAudioSource.Stop();
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().reloadGunSound;
        playerReloadAudioSource.clip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        playerReloadAudioSource.volume = sfxVolume * PlayerShoot.Instance.GetHeldGunSO().reloadSFXVolumeMultiplier;
        playerReloadAudioSource.Play();
    }


    private void PlayerMovement_OnPlayerRoll(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerRollAudioClips[Random.Range(0, playerRollAudioClips.Length)], sfxVolume*.7f);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDiedAudioClips[Random.Range(0, playerDiedAudioClips.Length)], sfxVolume);
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDamagedElectricAudioClips[Random.Range(0, playerDamagedElectricAudioClips.Length)], sfxVolume * .4f);

        if (Player.Instance.GetHP() == 0) return;
        StartCoroutine(PlayHumanDamagedAudioClip());
    }

    private IEnumerator PlayHumanDamagedAudioClip() {
        yield return new WaitForSeconds(.075f);
        playerAudioSource.PlayOneShot(playerDamagedAudioClips[Random.Range(0, playerDamagedAudioClips.Length)], sfxVolume);
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, System.EventArgs e) {
        exhaustedSFXPlaying = true;
        AudioClip audioclip = playerExhaustedAudioClips[Random.Range(0, playerExhaustedAudioClips.Length)];
        playerAudioSource.PlayOneShot(audioclip, sfxVolume * .5f);

        StartCoroutine(SetExhaustionSFXPlaying(audioclip.length));
    }
    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(footStepAudioClips[Random.Range(0, footStepAudioClips.Length)], sfxVolume);
    }

    private void PlayerAnimator_OnPantTriggered(object sender, System.EventArgs e) {
        if (exhaustedSFXPlaying) return;

        float volumeMultiplier = 1f;
        if (almostExhausted) {
            volumeMultiplier = .45f;
        } else {
            volumeMultiplier = .15f;
        }

        playerAudioSource.PlayOneShot(playerPantAudioClips[Random.Range(0, playerPantAudioClips.Length)], sfxVolume * volumeMultiplier);
    }
    private void ActiveMoveSpeedBoostVisual_OnMoveSpeedFootStepTriggered(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(activeMoveSpeedBoostFootstepAudioClip, sfxVolume / 8);
    }

    private IEnumerator SetExhaustionSFXPlaying(float sfxDuration) {
        yield return new WaitForSeconds(sfxDuration);
        exhaustedSFXPlaying = false;
    }

    private void OnDestroy() {
        GunMeleeAttackCollider.OnAnyGunMeleeAttackHit -= GunMeleeAttackCollider_OnAnyGunMeleeAttackHit;
    }
}
