using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager : MonoBehaviour
{
    private MobAttack mobAttack;
    private MobMovement mobMovement;
    private Animator animator;

    private bool moving;
    private float moveDir;
    private float watchDir;
    private float previousWatchDir = 1f;

    private void Awake() {
        mobMovement = GetComponentInParent<MobMovement>();
        mobAttack = GetComponentInParent<MobAttack>();
        animator = GetComponent<Animator>();

        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
    }

    private void Update() {
        moveDir = mobMovement.GetMoveDirFloat();

        HandleXScale();
        HandleAnimatorMovementBool();

    }

    private void HandleAnimatorMovementBool() {

        if (moveDir != 0) {

            if (!moving) {
                animator.SetBool("Walking", true);
            }

            moving = true;

        }
        else {
            if (moving) {
                animator.SetBool("Walking", false);
            }

            moving = false;

        }
    }

    private void HandleXScale() {

        if (moving) {
            HandleScaleChange(moveDir);
            return;
        }

        if (mobAttack.GetAttacking()) {
            HandleScaleChange(mobAttack.GetAttackDir().x);
            return;
        }

        HandleScaleChange(watchDir);
    }
    private void HandleScaleChange(float watchDir) {
        if (watchDir < 0 && previousWatchDir > 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(-1, 1, 1);
            transform.localScale = newScale;
        }

        if (watchDir > 0 && previousWatchDir < 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            transform.localScale = newScale;
        }
    }
    private void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.SetTrigger("Attack");
    }


}
