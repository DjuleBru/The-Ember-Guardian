using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestVisual : MonoBehaviour
{
    protected Chest chest;
    protected Animator animator;

    [SerializeField] protected bool showHoveringIndicator;
    [SerializeField] protected GameObject hoveringIndicatorGO;

    [SerializeField] protected SpriteRenderer chestSpriteRenderer;
    [SerializeField] protected Animator inputIconAnimator;
    [SerializeField] protected Material unhoveredMaterial;
    [SerializeField] protected Material hoveredMaterial;

    [SerializeField] protected RuntimeAnimatorController initialChestAnimator;
    [SerializeField] protected RuntimeAnimatorController ammoChestAnimator;
    [SerializeField] protected RuntimeAnimatorController gemChestAnimator;
    [SerializeField] protected RuntimeAnimatorController orbChestAnimator;
    [SerializeField] protected RuntimeAnimatorController hugeChestAnimator;
    [SerializeField] protected RuntimeAnimatorController weaponChestAnimator;
    [SerializeField] protected RuntimeAnimatorController skillChestAnimator;
    [SerializeField] protected RuntimeAnimatorController trapChestAnimator;

    protected void Awake() {
        chest = GetComponentInParent<Chest>();
        animator = GetComponent<Animator>();

        chest.OnPlayerTriggeredIn += Chest_OnPlayerTriggeredIn;
        chest.OnPlayerTriggeredOut += Chest_OnPlayerTriggeredOut;
        chest.OnChestOpenedAnimationOver += Chest_OnChestOpenedAnimationOver;
    }

    protected virtual void Start() {
        chest.OnChestOpened += Chest_OnChestOpened;
        chest.OnChestDisappear += Chest_OnChestDisappear;

        if (chest.GetChestType() == Chest.ChestType.orbChest) {
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
        if (chest.GetChestType() == Chest.ChestType.skillChest) {
            animator.runtimeAnimatorController = skillChestAnimator;
        }
        if (chest.GetChestType() == Chest.ChestType.trapChest) {
            animator.runtimeAnimatorController = trapChestAnimator;
        }
    }
    protected void Chest_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        chestSpriteRenderer.material = unhoveredMaterial;

        if (!chest.GetPayToOpenChest() || (chest.GetPayToOpenChest() && chest.GetChestOpened())) {
            inputIconAnimator.ResetTrigger("Show");
            inputIconAnimator.SetTrigger("Hide");
        }

        if(showHoveringIndicator) {
            hoveringIndicatorGO.SetActive(true);
        }
    }

    protected void Chest_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        chestSpriteRenderer.material = hoveredMaterial;

        if(!chest.GetPayToOpenChest() || (chest.GetPayToOpenChest() && chest.GetChestOpened())) {
            inputIconAnimator.ResetTrigger("Hide");
            inputIconAnimator.SetTrigger("Show");
        }

        if (showHoveringIndicator) {
            hoveringIndicatorGO.SetActive(false);
        }
    }


    protected void Chest_OnChestDisappear(object sender, System.EventArgs e) {
        animator.SetTrigger("Disappear");
        inputIconAnimator.gameObject.SetActive(false);
    }

    protected void Chest_OnChestOpened(object sender, System.EventArgs e) {
        animator.SetTrigger("Opened");
        inputIconAnimator.gameObject.SetActive(false);
        chestSpriteRenderer.material = unhoveredMaterial;

    }
    protected void Chest_OnChestOpenedAnimationOver(object sender, System.EventArgs e) {

        if (!chest.GetChestDisappearsAutomatically()) {
            inputIconAnimator.gameObject.SetActive(true);
            animator.SetTrigger("Opened_Idle");
            inputIconAnimator.ResetTrigger("Hide");
            inputIconAnimator.SetTrigger("Show");
        }

    }

}
