using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player aggroFeedbacks;
    [SerializeField] private MMF_Player enteredLightFeedbacks;

    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private Creature creature;

    private float minAggroYForce = 4f;
    private float maxAggroYForce = 6f;

    private bool died;
    private bool enteredLight;
    private float enteredLightFeedbacksRate = .7f;
    private float enteredLightFeedbacksTimer;

    private void Awake() {
        creatureAI.OnCreatureAggro += CreatureAI_OnCreatureAggro;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureDied += Creature_OnCreatureDied;
    }

    private void Creature_OnCreatureDied(object sender, System.EventArgs e) {
        died = true;
    }

    private void Update() {
        if(enteredLight) {
            enteredLightFeedbacksTimer -= Time.deltaTime;
            if(enteredLightFeedbacksTimer < 0 ) {
                enteredLightFeedbacksTimer = enteredLightFeedbacksRate;
                enteredLightFeedbacks.PlayFeedbacks();
            }
        }
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        enteredLight = false;
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        enteredLight = true;
    }


    private void CreatureAI_OnCreatureAggro(object sender, System.EventArgs e) {
        if (died) return;
        aggroFeedbacks.PlayFeedbacks();
    }

}
