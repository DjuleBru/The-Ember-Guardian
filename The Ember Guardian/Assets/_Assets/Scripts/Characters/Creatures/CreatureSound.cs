using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : SoundObject
{
    private AudioSource creatureAudioSource;
    private CreatureSO creatureSO;
    [SerializeField] private AudioSource creatureIdleAudioSource;

    [SerializeField] private Creature creature;
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private CreatureAttack creatureAttack;
    [SerializeField] private CreatureAnimatorManager creatureAnimator;

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
        creatureAnimator.OnFootStepTriggered += CreatureAnimator_OnFootStepTriggered;

        creatureSO = creature.GetCreatureSO();

        if (IsTooFarFromPlayer()) return;
        creatureAudioSource.PlayOneShot(creatureSO.spawnAudioClips[Random.Range(0, creatureSO.spawnAudioClips.Length)], creatureSO.spawnVolumeMultiplier * sfxVolume);
        
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
        if (distanceToPlayer > 15f) return true;
        return false;
    }
}
