using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestVisual : MonoBehaviour
{
    private Chest chest;
    private Animator animator;

    [SerializeField] private bool showHoveringIndicator;
    [SerializeField] private GameObject hoveringIndicatorGO;

    [SerializeField] private SpriteRenderer chestSpriteRenderer;
    [SerializeField] private Animator inputIconAnimator;
    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;

    [SerializeField] private RuntimeAnimatorController initialChestAnimator;
    [SerializeField] private RuntimeAnimatorController ammoChestAnimator;
    [SerializeField] private RuntimeAnimatorController gemChestAnimator;
    [SerializeField] private RuntimeAnimatorController orbChestAnimator;
    [SerializeField] private RuntimeAnimatorController hugeChestAnimator;
    [SerializeField] private RuntimeAnimatorController weaponChestAnimator;

    private void Awake() {
        chest = GetComponentInParent<Chest>();
        animator = GetComponent<Animator>();

        chest.OnPlayerTriggeredIn += Chest_OnPlayerTriggeredIn;
        chest.OnPlayerTriggeredOut += Chest_OnPlayerTriggeredOut;
        chest.OnChestOpenedAnimationOver += Chest_OnChestOpenedAnimationOver;
    }

    private void Chest_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        inputIconAnimator.ResetTrigger("Show");
        inputIconAnimator.SetTrigger("Hide");
        chestSpriteRenderer.material = unhoveredMaterial; 

        if(showHoveringIndicator) {
            hoveringIndicatorGO.SetActive(true);
        }
    }

    private void Chest_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        inputIconAnimator.ResetTrigger("Hide");
        inputIconAnimator.SetTrigger("Show");
        chestSpriteRenderer.material = hoveredMaterial;

        if (showHoveringIndicator) {
            hoveringIndicatorGO.SetActive(false);
        }
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
        if (chest.GetChestType() == Chest.ChestType.weaponChest) {
            animator.runtimeAnimatorController = weaponChestAnimator;
        }
    }

    private void Chest_OnChestDisappear(object sender, System.EventArgs e) {
        animator.SetTrigger("Disappear");
        inputIconAnimator.gameObject.SetActive(false);
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        animator.SetTrigger("Opened");
        inputIconAnimator.gameObject.SetActive(false);
        chestSpriteRenderer.material = unhoveredMaterial;

    }
    private void Chest_OnChestOpenedAnimationOver(object sender, System.EventArgs e) {

        if (!chest.GetChestDisappearsAutomatically()) {
            inputIconAnimator.gameObject.SetActive(true);
            animator.SetTrigger("Opened_Idle");
            inputIconAnimator.ResetTrigger("Hide");
            inputIconAnimator.SetTrigger("Show");
        }

    }

}
