using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectileFeedbacks : MonoBehaviour
{
    [SerializeField] private StaticProjectile staticProjectile;
    [SerializeField] private bool takeDistanceInAccountForFeedbacks;
    [SerializeField] private float maxDistanceToPlayFeedbacks;

    [SerializeField] private bool onCreatureHitPlayFeedbacks;
    [SerializeField] private MMF_Player hitCreatureFeedbacks;
    [SerializeField] private MMF_Player trapTriggerFeedbacks;
    [SerializeField] private float minDelayBetweenFeedbackPlays = .2f;

    private bool justHitCreature;
    private float justHitCreatureTimer;

    private void Start() {
        staticProjectile.OnTrapTriggered += StaticProjectile_OnTrapTriggered;
        if (!onCreatureHitPlayFeedbacks) return;
        staticProjectile.OnStaticProjectileHitCreature += StaticProjectile_OnStaticProjectileHitCreature;
    }


    private void Update() {
        if (justHitCreature) {
            justHitCreatureTimer -= Time.deltaTime;
            if (justHitCreatureTimer < 0) {
                justHitCreature = false;
            }
        }
    }
    private void StaticProjectile_OnStaticProjectileHitCreature(object sender, System.EventArgs e) {
        if (justHitCreature) return;

        Debug.Log("StaticProjectile_OnStaticProjectileHitCreature");
        hitCreatureFeedbacks.PlayFeedbacks();
        justHitCreature = true;
    }

    private void StaticProjectile_OnTrapTriggered(object sender, System.EventArgs e) {
        if(takeDistanceInAccountForFeedbacks) {
            Debug.Log(Mathf.Abs(transform.position.x - Player.Instance.transform.position.x));
            if (Mathf.Abs(transform.position.x - Player.Instance.transform.position.x) > maxDistanceToPlayFeedbacks) {
                return;
            } 
        }

        Debug.Log("PlayFeedbacks");
        trapTriggerFeedbacks.PlayFeedbacks();
    }

}
