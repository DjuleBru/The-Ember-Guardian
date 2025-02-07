using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubChestVisual : MonoBehaviour
{
    private HubChest hubChest;
    private Animator animator;
    private void Awake() {
        hubChest = GetComponentInParent<HubChest>();
        animator = GetComponent<Animator>();
    }
    private void Start() {
        hubChest.OnChestOpened += Chest_OnChestOpened;
        hubChest.OnChestClosed += HubChest_OnChestClosed;
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        animator.SetTrigger("Close");
        animator.ResetTrigger("Opened");
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        animator.SetTrigger("Opened");
        animator.ResetTrigger("Close");
    }
}
