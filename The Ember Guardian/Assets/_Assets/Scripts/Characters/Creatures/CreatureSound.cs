using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : SoundObject
{
    protected AudioSource creatureAudioSource;
    protected CreatureSO creatureSO;

    protected bool attackSFXHandledByAnimation;
    [SerializeField] protected bool canHearCreatureOutsideScreen;
    [SerializeField] protected AudioSource creatureIdleAudioSource;

    [SerializeField] protected Creature creature;
    [SerializeField] protected CreatureAI creatureAI;
    [SerializeField] protected CreatureAttack creatureAttack;
    [SerializeField] protected CreatureAnimatorManager creatureAnimator;
    [SerializeField] protected CreatureAnimatorSounds creatureAnimatorSounds;

    [SerializeField] protected AudioClip[] enteredLightAudioClips;

    protected float attackSFXDelayAfterAnimationStart;

    protected bool diedRecently;

    protected void Awake() {
        creatureAudioSource = GetComponent<AudioSource>();
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

        creatureAnimatorSounds.OnAttackStartedCharging += CreatureAnimatorSounds_OnAttackStartedCharging;
        creatureAnimatorSounds.OnChargedAttackReleased += CreatureAnimatorSounds_OnChargedAttackReleased;

        creatureSO = creature.GetCreatureSO();
        attackSFXHandledByAnimation = creatureSO.attackSFXHandledByAnimation;
        attackSFXDelayAfterAnimationStart = creatureSO.attackSFXDelayAfterAnimationStart;

        if (IsTooFarFromPlayer()) return;
        AudioClip audioClip = creatureSO.spawnAudioClips[Random.Range(0, creatureSO.spawnAudioClips.Length)];

        creatureAudioSource.PlayOneShot(audioClip, creatureSO.spawnVolumeMultiplier * sfxVolume);
    }

    protected void CreatureAnimatorSounds_OnChargedAttackReleased(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        if (creatureSO.attackReleasedAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackReleasedAudioClips[Random.Range(0, creatureSO.attackReleasedAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    protected void CreatureAnimatorSounds_OnAttackStartedCharging(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        if (creatureSO.attackStartedChargingAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackStartedChargingAudioClips[Random.Range(0, creatureSO.attackStartedChargingAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    protected void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {
        if (attackSFXHandledByAnimation) return;
        if (IsTooFarFromPlayer()) return;
        StartCoroutine(PlayAttackSFXAfterDelay(attackSFXDelayAfterAnimationStart));
    }

    protected IEnumerator PlayAttackSFXAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        if (creatureSO.attackAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackAudioClips[Random.Range(0, creatureSO.attackAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    protected void CreatureAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(creatureSO.attackHitAudioClips[Random.Range(0, creatureSO.attackHitAudioClips.Length)], creatureSO.attackHitVolumeMultiplier * sfxVolume);
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
    }

    protected bool IsTooFarFromPlayer() {
        if (canHearCreatureOutsideScreen) return false;
        float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        if (distanceToPlayer > 20f) return true;
        return false;
    }
}
