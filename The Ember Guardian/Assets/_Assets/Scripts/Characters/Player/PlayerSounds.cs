using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : SoundObject
{
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource playerReloadAudioSource;

    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private AudioClip[] playerDamagedAudioClips;
    [SerializeField] private AudioClip[] playerDamagedAudioClips_Female;
    [SerializeField] private AudioClip[] playerDiedAudioClips;
    [SerializeField] private AudioClip[] playerDiedAudioClips_Female;
    [SerializeField] private AudioClip[] playerDamagedElectricAudioClips;
    [SerializeField] private AudioClip[] playerPantAudioClips;
    [SerializeField] private AudioClip[] playerPantAudioClips_Female;
    [SerializeField] private AudioClip[] playerExhaustedAudioClips;
    [SerializeField] private AudioClip[] playerExhaustedAudioClips_Female;
    [SerializeField] private AudioClip[] playerRollAudioClips;
    [SerializeField] private AudioClip[] playerMeleeAttackStartedAudioClips;
    [SerializeField] private AudioClip[] playerMeleeAttackHitAudioClips;
    [SerializeField] private AudioClip[] gunJamHitFailed;
    [SerializeField] private AudioClip[] gunJamSpamHitPerformed;
    [SerializeField] private AudioClip[] gunJamPerfectQTE;
    [SerializeField] private AudioClip activeMoveSpeedBoostFootstepAudioClip;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerBreathAnimator playerBreathAnimator;
    [SerializeField] private AnimationCurve pitchWithSpeedAnimationCurve;
    [SerializeField] private ActiveMoveSpeedBoostVisual activeMoveSpeedBoostVisual;

    private AudioClip[] playerDamagedAudioClips_selectedGender;
    private AudioClip[] playerDiedAudioClips_selectedGender;
    private AudioClip[] playerPantAudioClips_selectedGender;
    private AudioClip[] playerExhaustedAudioClips_selectedGender;

    private bool almostExhausted;
    private bool exhaustedSFXPlaying;
    private float pausedReloadAudioTime = 0f;

    protected override void Start() {
        base.Start();

        bool isFemaleAnimator = ES3.Load("characterType", false);
        SetSelectedGenderAudioClips(isFemaleAnimator);

        playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
        playerBreathAnimator.OnPantTriggered += PlayerAnimator_OnPantTriggered;

        GunJamHandler.OnAnyCorrectJamSequenceInput += GunJamHandler_OnAnyCorrectJamSequenceInput;
        GunJamHandler.OnAnyJamWrongInput += GunJamHandler_OnAnyJamSequenceFailed;
        GunJamHandler.OnAnySpamButtonPressed += GunJamHandler_OnAnySpamButtonPressed;
        GunJamHandler.OnAnyPerfectJamSequenceCompleted += GunJamHandler_OnAnyPerfectJamSequenceCompleted;
        Gun.OnAnySurgeReloadSuccess += Gun_OnAnyGunJamRepaired;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded += PlayerShoot_OnPlayerReloadInterruptedEnded;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionDeactivateFeedbacks += PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks;

        PlayerMeleeAttack.Instance.OnMeleeAttackStarted += PlayerMeleeAttack_OnMeleeAttackStarted;
        GunMeleeAttackCollider.OnAnyGunMeleeAttackHit += GunMeleeAttackCollider_OnAnyGunMeleeAttackHit;

        activeMoveSpeedBoostVisual.OnMoveSpeedFootStepTriggered += ActiveMoveSpeedBoostVisual_OnMoveSpeedFootStepTriggered;
    }

    private void SetSelectedGenderAudioClips(bool isFemale) {
        if(isFemale) {
            playerDamagedAudioClips_selectedGender = playerDamagedAudioClips_Female;
            playerDiedAudioClips_selectedGender = playerDiedAudioClips_Female;
            playerPantAudioClips_selectedGender = playerPantAudioClips_Female;
            playerExhaustedAudioClips_selectedGender = playerExhaustedAudioClips_Female;

        } else {
            playerDamagedAudioClips_selectedGender = playerDamagedAudioClips;
            playerDiedAudioClips_selectedGender = playerDiedAudioClips;
            playerPantAudioClips_selectedGender = playerPantAudioClips;
            playerExhaustedAudioClips_selectedGender = playerExhaustedAudioClips;
        }
    }

    private void GunJamHandler_OnAnyPerfectJamSequenceCompleted(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(gunJamPerfectQTE[Random.Range(0, gunJamPerfectQTE.Length)], sfxVolume * .75f);
    }
    private void Gun_OnAnyGunJamRepaired(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(gunJamPerfectQTE[Random.Range(0, gunJamPerfectQTE.Length)], sfxVolume * .75f);
    }


    private void GunJamHandler_OnAnySpamButtonPressed(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(gunJamSpamHitPerformed[Random.Range(0, gunJamSpamHitPerformed.Length)], sfxVolume * .5f);
    }

    private void GunJamHandler_OnAnyJamSequenceFailed(object sender, GunJamHandler.OnAnyJamSequenceProgressedEventArgs e) {
        playerAudioSource.PlayOneShot(gunJamHitFailed[Random.Range(0, gunJamHitFailed.Length)], sfxVolume * .7f);
    }

    private void GunJamHandler_OnAnyCorrectJamSequenceInput(object sender, System.EventArgs e) {
        AudioClip[] gunJamHitProgress = PlayerShoot.Instance.GetHeldGunSO().gunJammHitProgressSound;
        float volumeMultiplier = PlayerShoot.Instance.GetHeldGunSO().gunJamHitProgressVolumeMultiplier;
        playerAudioSource.PlayOneShot(gunJamHitProgress[Random.Range(0, gunJamHitProgress.Length)], sfxVolume * volumeMultiplier);
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
        pausedReloadAudioTime = playerReloadAudioSource.time;
        playerReloadAudioSource.Pause();
    }

    private void PlayerShoot_OnPlayerReloadInterruptedEnded(object sender, System.EventArgs e) {
        playerReloadAudioSource.time = pausedReloadAudioTime;
        playerReloadAudioSource.Play();
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        playerReloadAudioSource.time = 0;
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().reloadGunSound;
        playerReloadAudioSource.clip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        playerReloadAudioSource.volume = sfxVolume * PlayerShoot.Instance.GetHeldGunSO().reloadSFXVolumeMultiplier;
        playerReloadAudioSource.Play();
    }


    private void PlayerMovement_OnPlayerRoll(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerRollAudioClips[Random.Range(0, playerRollAudioClips.Length)], sfxVolume*.7f);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDiedAudioClips_selectedGender[Random.Range(0, playerDiedAudioClips_selectedGender.Length)], sfxVolume);
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        playerAudioSource.PlayOneShot(playerDamagedElectricAudioClips[Random.Range(0, playerDamagedElectricAudioClips.Length)], sfxVolume * .4f);

        if (Player.Instance.GetHP() == 0) return;
        StartCoroutine(PlayHumanDamagedAudioClip());
    }

    private IEnumerator PlayHumanDamagedAudioClip() {
        yield return new WaitForSeconds(.075f);
        playerAudioSource.PlayOneShot(playerDamagedAudioClips_selectedGender[Random.Range(0, playerDamagedAudioClips_selectedGender.Length)], sfxVolume);
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, System.EventArgs e) {
        exhaustedSFXPlaying = true;
        AudioClip audioclip = playerExhaustedAudioClips_selectedGender[Random.Range(0, playerExhaustedAudioClips_selectedGender.Length)];
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

        playerAudioSource.PlayOneShot(playerPantAudioClips_selectedGender[Random.Range(0, playerPantAudioClips_selectedGender.Length)], sfxVolume * volumeMultiplier);
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
        PlayerMeleeAttack.Instance.OnMeleeAttackStarted -= PlayerMeleeAttack_OnMeleeAttackStarted;
        GunJamHandler.OnAnyCorrectJamSequenceInput -= GunJamHandler_OnAnyCorrectJamSequenceInput;
        GunJamHandler.OnAnyJamWrongInput -= GunJamHandler_OnAnyJamSequenceFailed;
        GunJamHandler.OnAnySpamButtonPressed -= GunJamHandler_OnAnySpamButtonPressed;
        GunJamHandler.OnAnyPerfectJamSequenceCompleted -= GunJamHandler_OnAnyPerfectJamSequenceCompleted;
        Gun.OnAnySurgeReloadSuccess -= Gun_OnAnyGunJamRepaired;
    }
}
