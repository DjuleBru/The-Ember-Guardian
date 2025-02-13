using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialAreaBlockingCollider : MonoBehaviour
{
    [SerializeField] private TrialArea trialArea;
    private Animator wallAnimator;
    private Collider2D blockingCollider;


    private void Start() {
        blockingCollider = GetComponent<Collider2D>();
        wallAnimator = GetComponent<Animator>();

        trialArea.OnTrialCompleted += TrialArea_OnTrialCompleted;
        trialArea.OnTrialFailed += TrialArea_OnTrialFailed;
        trialArea.OnTrialWallsLifted += TrialArea_OnTrialWallsLifted;

        blockingCollider.enabled = false;
    }

    private void TrialArea_OnTrialWallsLifted(object sender, System.EventArgs e) {
        LiftUpWall();
    }

    private void TrialArea_OnTrialFailed(object sender, System.EventArgs e) {
        BringDownWall();
    }

    private void TrialArea_OnTrialCompleted(object sender, System.EventArgs e) {
        BringDownWall();
    }

    private void LiftUpWall() {
        blockingCollider.enabled = true;
        wallAnimator.SetTrigger("MoveUp");
    }
    private void BringDownWall() {
        blockingCollider.enabled = false;
        wallAnimator.SetTrigger("MoveDown");
    }
}
