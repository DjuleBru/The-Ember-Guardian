using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureStatusEffectsVisuals : MonoBehaviour
{

    private Creature creature;
    [SerializeField] private GameObject fireLightDebuffedGameObject;
    [SerializeField] private GameObject shockedGameObject;
    [SerializeField] private GameObject poisonedGameObject;
    [SerializeField] private GameObject immobilizedGameObject;

    protected void Awake() {
        fireLightDebuffedGameObject.SetActive(false);
        shockedGameObject.SetActive(false);
        poisonedGameObject.SetActive(false);
        immobilizedGameObject.SetActive(false);

        creature = GetComponentInParent<Creature>();
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureImmobilizedStarted += Creature_OnCreatureImmobilizedStarted;
        creature.OnCreatureImmobilizedStopped += Creature_OnCreatureImmobilizedStopped;
        creature.OnCreaturePoisonedStarted += Creature_OnCreaturePoisonedStarted;
        creature.OnCreaturePoisoneStopped += Creature_OnCreaturePoisoneStopped;
        creature.OnCreatureShockedStarted += Creature_OnCreatureShockedStarted;
        creature.OnCreatureShockedStopped += Creature_OnCreatureShockedStopped;
    }

    private void Creature_OnCreatureShockedStopped(object sender, System.EventArgs e) {
        shockedGameObject.SetActive(false);
    }

    private void Creature_OnCreatureShockedStarted(object sender, System.EventArgs e) {
        shockedGameObject.SetActive(true);
    }

    private void Creature_OnCreaturePoisoneStopped(object sender, System.EventArgs e) {
        poisonedGameObject.SetActive(false);
    }

    private void Creature_OnCreaturePoisonedStarted(object sender, System.EventArgs e) {
        poisonedGameObject.SetActive(true);
    }

    private void Creature_OnCreatureImmobilizedStopped(object sender, System.EventArgs e) {
        immobilizedGameObject.SetActive(false);
    }

    private void Creature_OnCreatureImmobilizedStarted(object sender, System.EventArgs e) {
        immobilizedGameObject.SetActive(true);
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        fireLightDebuffedGameObject.SetActive(false);
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        fireLightDebuffedGameObject.SetActive(true);
    }
}
