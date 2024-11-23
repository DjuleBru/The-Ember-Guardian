using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFeedbacks : MonoBehaviour
{
    private Fire fire;
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player ectractingEmberFeedbacks;

    private void Awake() {
        fire = GetComponentInParent<Fire>();
    }

    private void Start() {
        fire.OnFireDamageTaken += Fire_OnFireDamageTaken;
        fire.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
        fire.OnFireEmberExtractionStopped += Fire_OnFireEmberExtractionStopped;
    }

    private void Fire_OnFireEmberExtractionStopped(object sender, System.EventArgs e) {
        ectractingEmberFeedbacks.StopFeedbacks();
        ectractingEmberFeedbacks.StopAllCoroutines();
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        ectractingEmberFeedbacks.PlayFeedbacks();
    }

    private void Fire_OnFireDamageTaken(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
