using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound : MonoBehaviour
{
    private AudioSource creatureAudioSource;

    [SerializeField] private Creature creature;
    [SerializeField] private CreatureAI creatureAI;

    [SerializeField] private AudioClip[] enteredLightAudioClips;

    private bool diedRecently;

    private void Awake() {
        creatureAudioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureDied += Creature_OnAnyCreatureDied;
        creatureAI.OnCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        creature.OnCreatureIdleSoundTriggered += Creature_OnAnyCreatureIdleSoundTriggered;
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        creatureAudioSource.PlayOneShot(enteredLightAudioClips[Random.Range(0, enteredLightAudioClips.Length)]);
    }

    private void Creature_OnAnyCreatureIdleSoundTriggered(object sender, System.EventArgs e) {
        if (diedRecently) return;

        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().idleAudioClips[Random.Range(0, creature.GetCreatureSO().idleAudioClips.Length)], .5f);
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (diedRecently) return;

        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().aggroAudioClips[Random.Range(0, creature.GetCreatureSO().aggroAudioClips.Length)], .5f);
    }

    private void Creature_OnAnyCreatureDied(object sender, System.EventArgs e) {
        diedRecently = true;
        creatureAudioSource.PlayOneShot(creature.GetCreatureSO().dieAudioClips[Random.Range(0, creature.GetCreatureSO().dieAudioClips.Length)], .75f);
    }
}
