using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class ChestVisual : MonoBehaviour
{
    private Chest chest;
    private Animator animator;

    [SerializeField] private RuntimeAnimatorController initialChestAnimator;
    [SerializeField] private RuntimeAnimatorController ammoChestAnimator;
    [SerializeField] private RuntimeAnimatorController gemChestAnimator;
    [SerializeField] private RuntimeAnimatorController orbChestAnimator;
    [SerializeField] private RuntimeAnimatorController hugeChestAnimator;

    private void Awake() {
        chest = GetComponentInParent<Chest>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        chest.OnChestOpened += Chest_OnChestOpened;
        chest.OnChestDisappear += Chest_OnChestDisappear;

        if(chest.GetChestType() == Chest.ChestType.orbChest) {
            animator.runtimeAnimatorController = orbChestAnimator;
        }
        if (chest.GetChestType() == Chest.ChestType.ammoChest) {
            animator.runtimeAnimatorController = ammoChestAnimator;
        }
        if (chest.GetChestType() == Chest.ChestType.initialChest) {
            animator.runtimeAnimatorController = initialChestAnimator;
        }
        if (chest.GetChestType() == Chest.ChestType.hugeChest) {
            animator.runtimeAnimatorController = hugeChestAnimator;
        }
        if (chest.GetChestType() == Chest.ChestType.gemChest) {
            animator.runtimeAnimatorController = gemChestAnimator;
        }
    }

    private void Chest_OnChestDisappear(object sender, System.EventArgs e) {
        animator.SetTrigger("Disappear");
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        animator.SetTrigger("Opened");
    }
}
