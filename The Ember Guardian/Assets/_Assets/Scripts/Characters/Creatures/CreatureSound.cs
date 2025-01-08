using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : MonoBehaviour
{
    private AudioSource creatureAudioSource;
    [SerializeField] private AudioSource creatureIdleAudioSource;

    [SerializeField] private Creature creature;
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private CreatureAttack creatureAttack;
    [SerializeField] private CreatureAnimatorManager creatureAnimator;

    [SerializeField] private AudioClip[] enteredLightAudioClips;

    private bool diedRecently;
    private float sfxVolume;

    private void Awake() {
        creatureAudioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureDied += Creature_OnAnyCreatureDied;
        creatureAI.OnCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        creature.OnCreatureIdleSoundTriggered += Creature_OnAnyCreatureIdleSoundTriggered;
        creatureAttack.OnMobAttackHit += CreatureAttack_OnMobAttackHit;
        creatureAnimator.OnFootStepTriggered += CreatureAnimator_OnFootStepTriggered;
    }

    private void CreatureAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().attackHitAudioClips[Random.Range(0, creature.GetCreatureSO().attackHitAudioClips.Length)], .5f * sfxVolume);
    }

    private void CreatureAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().footStepAudioClips[Random.Range(0, creature.GetCreatureSO().footStepAudioClips.Length)], .2f * sfxVolume);
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        creatureAudioSource.PlayOneShot(enteredLightAudioClips[Random.Range(0, enteredLightAudioClips.Length)], sfxVolume);
    }

    private void Creature_OnAnyCreatureIdleSoundTriggered(object sender, System.EventArgs e) {
        if (diedRecently) return;
        creatureIdleAudioSource.PlayOneShot(creature.GetCreatureSO().idleAudioClips[Random.Range(0, creature.GetCreatureSO().idleAudioClips.Length)], .5f * sfxVolume);
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (diedRecently) return;

        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().aggroAudioClips[Random.Range(0, creature.GetCreatureSO().aggroAudioClips.Length)], .5f * sfxVolume);
    }

    private void Creature_OnAnyCreatureDied(object sender, System.EventArgs e) {
        diedRecently = true;
        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().dieAudioClips[Random.Range(0, creature.GetCreatureSO().dieAudioClips.Length)], .75f* sfxVolume);
    }
}
