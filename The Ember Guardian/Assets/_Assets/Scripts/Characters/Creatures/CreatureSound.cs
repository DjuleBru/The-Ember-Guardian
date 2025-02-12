using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : SoundObject
{
    private AudioSource creatureAudioSource;
    private CreatureSO creatureSO;

    private bool attackSFXHandledByAnimation;
    [SerializeField] private AudioSource creatureIdleAudioSource;

    [SerializeField] private Creature creature;
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private CreatureAttack creatureAttack;
    [SerializeField] private CreatureAnimatorManager creatureAnimator;
    [SerializeField] private CreatureAnimatorSounds creatureAnimatorSounds;

    [SerializeField] private AudioClip[] enteredLightAudioClips;

    [SerializeField] private float footstepVolumeMultiplier = .2f;
    [SerializeField] private float attackHitVolumeMultiplier = .5f;
    [SerializeField] private float idleVolumeMultiplier = .5f;
    [SerializeField] private float aggroVolumeMultiplier = .5f;
    [SerializeField] private float dieVolumeMultiplier = .75f;
    [SerializeField] private float spawnVolumeMultiplier = .75f;

    private bool diedRecently;

    private void Awake() {
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

        if (IsTooFarFromPlayer()) return;
        AudioClip audioClip = creatureSO.spawnAudioClips[Random.Range(0, creatureSO.spawnAudioClips.Length)];

        creatureAudioSource.PlayOneShot(audioClip, creatureSO.spawnVolumeMultiplier * sfxVolume);
        
    }

    private void CreatureAnimatorSounds_OnChargedAttackReleased(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        if (creatureSO.attackReleasedAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackReleasedAudioClips[Random.Range(0, creatureSO.attackReleasedAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    private void CreatureAnimatorSounds_OnAttackStartedCharging(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        if (creatureSO.attackStartedChargingAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackStartedChargingAudioClips[Random.Range(0, creatureSO.attackStartedChargingAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    private void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {
        if (attackSFXHandledByAnimation) return;
        if (IsTooFarFromPlayer()) return;
        if(creatureSO.attackAudioClips.Length > 0) {
            creatureAudioSource.PlayOneShot(creatureSO.attackAudioClips[Random.Range(0, creatureSO.attackAudioClips.Length)], creatureSO.attackVolumeMultiplier * sfxVolume);
        }
    }

    private void CreatureAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(creatureSO.attackHitAudioClips[Random.Range(0, creatureSO.attackHitAudioClips.Length)], creatureSO.attackHitVolumeMultiplier * sfxVolume);
    }

    private void CreatureAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(creatureSO.footStepAudioClips[Random.Range(0, creatureSO.footStepAudioClips.Length)], creatureSO.footstepVolumeMultiplier * sfxVolume);
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(enteredLightAudioClips[Random.Range(0, enteredLightAudioClips.Length)], sfxVolume);
    }

    private void Creature_OnAnyCreatureIdleSoundTriggered(object sender, System.EventArgs e) {
        if (diedRecently) return;
        if (creatureSO.idleAudioClips.Length == 0) return;
        if (IsTooFarFromPlayer()) return;

        creatureIdleAudioSource.PlayOneShot(creatureSO.idleAudioClips[Random.Range(0, creatureSO.idleAudioClips.Length)], creatureSO.idleVolumeMultiplier * sfxVolume);
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (diedRecently) return;
        if (IsTooFarFromPlayer()) return;

        creatureAudioSource.PlayOneShot(creatureSO.aggroAudioClips[Random.Range(0, creatureSO.aggroAudioClips.Length)], creatureSO.aggroVolumeMultiplier * sfxVolume);
    }

    private void Creature_OnAnyCreatureDied(object sender, System.EventArgs e) {
        diedRecently = true;
        if (IsTooFarFromPlayer()) return;

        creatureAudioSource.PlayOneShot(creatureSO.dieAudioClips[Random.Range(0, creatureSO.dieAudioClips.Length)], creatureSO.dieVolumeMultiplier * sfxVolume);
    }

    private bool IsTooFarFromPlayer() {
        float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        if (distanceToPlayer > 20f) return true;
        return false;
    }
}
