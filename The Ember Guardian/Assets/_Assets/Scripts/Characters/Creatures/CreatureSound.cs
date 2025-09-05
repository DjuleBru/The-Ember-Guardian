using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : SoundObject
{
    protected AudioSource creatureAudioSource;
    [SerializeField] protected AudioSource creatureContinousAudioSource;
    protected CreatureSO creatureSO;

    [SerializeField] protected bool canHearCreatureOutsideScreen;
    [SerializeField] protected AudioSource creatureIdleAudioSource;

    [SerializeField] protected Creature creature;
    [SerializeField] protected CreatureAI creatureAI;
    [SerializeField] protected CreatureAttack creatureAttack;
    [SerializeField] protected CreatureAnimatorManager creatureAnimator;
    [SerializeField] protected CreatureAnimatorSounds creatureAnimatorSounds;
    protected CreatureMovement creatureMovement;

    [SerializeField] protected AudioClip[] enteredLightAudioClips;

    protected float attackSFXDelayAfterAnimationStart;

    protected bool diedRecently;

    protected virtual void Awake() {
        creatureAudioSource = GetComponent<AudioSource>();
        creatureMovement = creature.GetComponent<CreatureMovement>();
    }

    protected override void Start() {
        base.Start();
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureDied += Creature_OnAnyCreatureDied;
        creatureAI.OnCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        creature.OnCreatureIdleSoundTriggered += Creature_OnAnyCreatureIdleSoundTriggered;
        creatureAttack.OnMobAttackHit += CreatureAttack_OnMobAttackHit;
        creatureAttack.OnMobAttack += CreatureAttack_OnMobAttack;
        creatureAnimator.OnFootStepTriggered += CreatureAnimator_OnFootStepTriggered;
        creatureMovement.OnMovementStarted += CreatureMovement_OnMovementStarted;
        creatureMovement.OnMovementStopped += CreatureMovement_OnMovementStopped;
        creature.OnCreatureEnabled += Creature_OnCreatureEnabled;

        creatureAnimatorSounds.OnAttackStartedCharging += CreatureAnimatorSounds_OnAttackStartedCharging;
        creatureAnimatorSounds.OnChargedAttackReleased += CreatureAnimatorSounds_OnChargedAttackReleased;
        creatureAnimatorSounds.OnTriggerAttackSFX += CreatureAnimatorSounds_OnTriggerAttackSFX;

        creatureSO = creature.GetCreatureSO();

        if(creatureAttack.GetCurrentCreatureAttackSO() != null) {
            attackSFXDelayAfterAnimationStart = creatureAttack.GetCurrentCreatureAttackSO().attackSFXDelayAfterAnimationStart;
        }

        if(creatureSO.hasContinousAudioClip) {
            AudioClip continuousAudioClip = creatureSO.continuousAudioClip[UnityEngine.Random.Range(0, creatureSO.continuousAudioClip.Length)];
            creatureContinousAudioSource.clip = continuousAudioClip; 
            float maxStartTime = Mathf.Max(0f, continuousAudioClip.length - 0.1f); // Évite les bords pour ne pas couper trop court
            creatureContinousAudioSource.time = Random.Range(0f, maxStartTime);
            creatureContinousAudioSource.pitch = Random.Range(0.95f, 1.05f);
            creatureContinousAudioSource.Play();
        }

        if (creatureSO.hasContinousMovementAudioClip) {
            AudioClip continuousMovementAudioClip = creatureSO.continuousMovementAudioClip[UnityEngine.Random.Range(0, creatureSO.continuousMovementAudioClip.Length)];
            creatureContinousAudioSource.clip = continuousMovementAudioClip;
            float maxStartTime = Mathf.Max(0f, continuousMovementAudioClip.length - 0.1f); // Évite les bords pour ne pas couper trop court
        }

        if (IsTooFarFromPlayer()) return;
        if (creatureSO.spawnAudioClips.Length > 0) {
            AudioClip audioClip = creatureSO.spawnAudioClips[Random.Range(0, creatureSO.spawnAudioClips.Length)];
            creatureAudioSource.PlayOneShot(audioClip, creatureSO.spawnVolumeMultiplier * sfxVolume);
        }
    }

    private void Creature_OnCreatureEnabled(object sender, System.EventArgs e) {
       
    }

    private void CreatureMovement_OnMovementStopped(object sender, System.EventArgs e) {
        if (!creatureSO.hasContinousMovementAudioClip) return;
        creatureContinousAudioSource.Stop();
    }

    private void CreatureMovement_OnMovementStarted(object sender, System.EventArgs e) {
        if (!creatureSO.hasContinousMovementAudioClip) return;
        creatureContinousAudioSource.Play();
    }

    protected void CreatureAnimatorSounds_OnChargedAttackReleased(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        AudioClip[] audioClips = creatureAttack.GetCurrentCreatureAttackSO().attackReleasedAudioClips;
        if (audioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)], creatureAttack.GetCurrentCreatureAttackSO().attackVolumeMultiplier * sfxVolume);
        }
    }

    protected void CreatureAnimatorSounds_OnAttackStartedCharging(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        AudioClip[] audioClips = creatureAttack.GetCurrentCreatureAttackSO().attackStartedChargingAudioClips;
        if (audioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)], creatureAttack.GetCurrentCreatureAttackSO().attackVolumeMultiplier * sfxVolume);
        }
    }

    private void CreatureAnimatorSounds_OnTriggerAttackSFX(object sender, System.EventArgs e) {
        StartCoroutine(PlayAttackSFXAfterDelay(0));
    }

    protected void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {
        if (creatureAttack.GetCurrentCreatureAttackSO().attackSFXHandledByAnimation) return;
        if (IsTooFarFromPlayer()) return;

        if (creatureAttack.GetCurrentCreatureAttackSO() != null) {
            attackSFXDelayAfterAnimationStart = creatureAttack.GetCurrentCreatureAttackSO().attackSFXDelayAfterAnimationStart;
        }

        StartCoroutine(PlayAttackSFXAfterDelay(attackSFXDelayAfterAnimationStart));
    }

    protected IEnumerator PlayAttackSFXAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        AudioClip[] audioClips = creatureAttack.GetCurrentCreatureAttackSO().attackAudioClips;
        if (audioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)], creatureAttack.GetCurrentCreatureAttackSO().attackVolumeMultiplier * sfxVolume);
        }
    }

    protected void CreatureAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        AudioClip[] audioClips = creatureAttack.GetCurrentCreatureAttackSO().attackHitAudioClips;
        if (audioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)], creatureAttack.GetCurrentCreatureAttackSO().attackHitVolumeMultiplier * sfxVolume);
        }
       
    }

    protected void CreatureAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(creatureSO.footStepAudioClips[Random.Range(0, creatureSO.footStepAudioClips.Length)], creatureSO.footstepVolumeMultiplier * sfxVolume);
    }

    protected void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(enteredLightAudioClips[Random.Range(0, enteredLightAudioClips.Length)], sfxVolume*2.5f);
    }

    protected void Creature_OnAnyCreatureIdleSoundTriggered(object sender, System.EventArgs e) {
        if (diedRecently) return;
        if (creatureSO.idleAudioClips.Length == 0) return;
        if (IsTooFarFromPlayer()) return;

        if (creatureSO.idleAudioClips.Length == 0) return;
        creatureIdleAudioSource.PlayOneShot(creatureSO.idleAudioClips[Random.Range(0, creatureSO.idleAudioClips.Length)], creatureSO.idleVolumeMultiplier * sfxVolume);
    }

    protected void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (diedRecently) return;
        if (IsTooFarFromPlayer()) return;

        if (creatureSO.aggroAudioClips.Length == 0) return;
        creatureAudioSource.PlayOneShot(creatureSO.aggroAudioClips[Random.Range(0, creatureSO.aggroAudioClips.Length)], creatureSO.aggroVolumeMultiplier * sfxVolume);
    }

    protected void Creature_OnAnyCreatureDied(object sender, System.EventArgs e) {
        diedRecently = true;
        if (IsTooFarFromPlayer()) return;

        creatureAudioSource.PlayOneShot(creatureSO.dieAudioClips[Random.Range(0, creatureSO.dieAudioClips.Length)], creatureSO.dieVolumeMultiplier * sfxVolume);

        if(creatureContinousAudioSource != null) {
            creatureContinousAudioSource.Stop();
        }
    }

    protected bool IsTooFarFromPlayer() {
        if (canHearCreatureOutsideScreen) return false;
        float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        if (distanceToPlayer > 20f) return true;
        return false;
    }
}
