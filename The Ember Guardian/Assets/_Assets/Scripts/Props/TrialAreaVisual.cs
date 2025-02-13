using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialAreaVisual : MonoBehaviour
{
    [SerializeField] private TrialArea trialArea;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        trialArea.OnPlayerTriggeredOut += TrialArea_OnPlayerTriggeredOut;
        trialArea.OnPlayerTriggeredIn += TrialArea_OnPlayerTriggeredIn;
        trialArea.OnTrialPaid += TrialArea_OnTrialPaid;
    }

    private void TrialArea_OnTrialPaid(object sender, System.EventArgs e) {
        animator.SetTrigger("Hide");

    }

    private void TrialArea_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        animator.SetTrigger("Show");
    }

    private void TrialArea_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        animator.SetTrigger("Hide");
    }
}
