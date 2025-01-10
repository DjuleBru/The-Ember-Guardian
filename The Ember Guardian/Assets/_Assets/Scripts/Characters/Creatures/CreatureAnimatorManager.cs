using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager : MonoBehaviour
{
    protected Creature creature;
    protected CreatureAI creatureAI;
    protected MobAttack mobAttack;
    protected MobMovement mobMovement;
    protected Animator animator;

    protected bool moving;
    protected float moveDir;
    protected float watchDir;
    protected float previousWatchDir = 1f;
    protected float animatorSpeedMultiplier = 1f;
    protected float baseMovementAnimationSpeed;

    public event EventHandler OnFootStepTriggered;

    protected virtual void Awake() {
        creature = GetComponentInParent<Creature>();
        creatureAI = GetComponentInParent<CreatureAI>();
        mobMovement = GetComponentInParent<MobMovement>();
        mobAttack = GetComponentInParent<MobAttack>();
        animator = GetComponent<Animator>();

        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
        creature.OnMobDied += Creature_OnMobDied;
        mobMovement.OnMoveSpeedBuffChanged += MobMovement_OnMoveSpeedBuffChanged;
    }


    protected virtual void Start() {
        baseMovementAnimationSpeed = creature.GetCreatureSO().baseMovementAnimationSpeed;
        animatorSpeedMultiplier = baseMovementAnimationSpeed;
        animator.SetFloat("AnimationSpeedMultiplier", animatorSpeedMultiplier);

        if(creature.GetCreatureSO().hasSpawnAnimation) {
            animator.SetTrigger("Spawn");
        }
    }

    protected void MobMovement_OnMoveSpeedBuffChanged(object sender, MobMovement.OnMoveSpeedBuffedEventArgs e) {
        animatorSpeedMultiplier = baseMovementAnimationSpeed * e.moveSpeedBuff;
        animator.SetFloat("AnimationSpeedMultiplier", animatorSpeedMultiplier);
    }

    protected void Update() {
        moveDir = mobMovement.GetMoveDirFloat();

        HandleXScale();
        HandleAnimatorMovementBool();

    }

    protected void HandleAnimatorMovementBool() {
        if (creature.GetCreatureSO().flying) {
            moving = true;
            return;
        };

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

    protected void HandleXScale() {

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

    protected void HandleScaleChange(float watchDir) {
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

    protected void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.SetTrigger("Attack");
    }

    protected void Creature_OnMobDied(object sender, System.EventArgs e) {
        animator.SetTrigger("Die");
    }

    public void FootStepEvent() {
        OnFootStepTriggered?.Invoke(this, EventArgs.Empty);
    }
}
