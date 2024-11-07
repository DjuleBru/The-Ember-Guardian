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

    private bool enteredLight;
    private float enteredLightFeedbacksRate = .7f;
    private float enteredLightFeedbacksTimer;

    private void Awake() {
        creatureAI.OnCreatureAggro += CreatureAI_OnCreatureAggro;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
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
        aggroFeedbacks.PlayFeedbacks();
        AddVerticalForce();
    }

    private void AddVerticalForce() {
        float forceY = Random.Range(minAggroYForce, maxAggroYForce);
        Vector2 force = new Vector2(0, forceY);

        creatureAI.GetComponent<Rigidbody2D>().AddForce(force * creature.GetCreatureSO().mass, ForceMode2D.Impulse);
    }

}
