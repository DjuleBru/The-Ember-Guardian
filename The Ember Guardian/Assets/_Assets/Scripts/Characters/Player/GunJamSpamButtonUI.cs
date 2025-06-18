using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunJamSpamButtonUI : MonoBehaviour
{

    [SerializeField] private Animator animator;

    private void Start() {
        GunJamHandler.OnAnySpamButtonPressed += GunJamHandler_OnAnySpamButtonPressed;
        GunJamHandler.OnAnyTimingButtonPressed += GunJamHandler_OnAnyTimingButtonPressed;
    }

    private void GunJamHandler_OnAnyTimingButtonPressed(object sender, System.EventArgs e) {
        animator.SetTrigger("Press");
    }

    private void GunJamHandler_OnAnySpamButtonPressed(object sender, System.EventArgs e) {
        animator.SetTrigger("Press");
    }


    private void OnDestroy() {
        GunJamHandler.OnAnySpamButtonPressed -= GunJamHandler_OnAnySpamButtonPressed;
        GunJamHandler.OnAnyTimingButtonPressed -= GunJamHandler_OnAnyTimingButtonPressed;
    }
}
