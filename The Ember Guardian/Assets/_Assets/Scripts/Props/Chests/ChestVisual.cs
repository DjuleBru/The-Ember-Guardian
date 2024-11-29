using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestVisual : MonoBehaviour
{
    private Chest chest;
    private Animator animator;

    private void Awake() {
        chest = GetComponentInParent<Chest>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        chest.OnChestOpened += Chest_OnChestOpened;
        chest.OnChestDisappear += Chest_OnChestDisappear;
    }

    private void Chest_OnChestDisappear(object sender, System.EventArgs e) {
        animator.SetTrigger("Disappear");
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        animator.SetTrigger("Opened");
    }
}
