using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFeedbacks : MonoBehaviour
{
    [SerializeField] protected MMF_Player aggroFeedbacks;
    [SerializeField] protected MMF_Player enteredLightFeedbacks;
    [SerializeField] protected MMF_Player footStepFeedbacks;
    [SerializeField] protected MMF_Player attackHitFeedbacks;

    [SerializeField] protected CreatureAnimatorManager creatureAnimatorManager;
    [SerializeField] protected CreatureAI creatureAI;
    [SerializeField] protected Creature creature;
    [SerializeField] protected CreatureAttack creatureAttack;

    protected float minAggroYForce = 4f;
    protected float maxAggroYForce = 6f;

    protected bool died;
    protected bool enteredLight;
    protected float enteredLightFeedbacksRate = .7f;
    protected float enteredLightFeedbacksTimer;

    protected virtual void Awake() {
        creatureAI.OnCreatureAggro += CreatureAI_OnCreatureAggro;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureDied += Creature_OnCreatureDied;

        if(creatureAnimatorManager != null) {
            creatureAnimatorManager.OnFootStepTriggered += CreatureAnimatorManager_OnFootStepTriggered;
        }

        if (creatureAttack != null) {
            creatureAttack.OnMobAttackHit += CreatureAttach_OnMobAttackHit;
        }
    }

    protected void CreatureAttach_OnMobAttackHit(object sender, System.EventArgs e) {
        if (attackHitFeedbacks != null) {
            attackHitFeedbacks.PlayFeedbacks();
        }
    }

    protected void CreatureAnimatorManager_OnFootStepTriggered(object sender, System.EventArgs e) {
        if(footStepFeedbacks != null) {
            footStepFeedbacks.PlayFeedbacks();
        }
    }

    protected void Creature_OnCreatureDied(object sender, System.EventArgs e) {
        died = true;
    }

    protected void Update() {
        if(enteredLight) {
            enteredLightFeedbacksTimer -= Time.deltaTime;
            if(enteredLightFeedbacksTimer < 0 ) {
                enteredLightFeedbacksTimer = enteredLightFeedbacksRate;
                enteredLightFeedbacks.PlayFeedbacks();
            }
        }
    }

    protected void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        enteredLight = false;
    }

    protected void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        enteredLight = true;
    }


    protected void CreatureAI_OnCreatureAggro(object sender, System.EventArgs e) {
        if (died) return;
        aggroFeedbacks.PlayFeedbacks();
    }

}
