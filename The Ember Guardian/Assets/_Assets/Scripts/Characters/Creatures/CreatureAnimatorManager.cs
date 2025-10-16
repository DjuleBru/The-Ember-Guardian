using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager : MonoBehaviour
{
    protected Creature creature;
    protected CreatureAI creatureAI;
    protected CreatureAttack creatureAttack;
    protected MobMovement mobMovement;
    protected Animator animator;

    protected bool spawned;
    protected bool moving;
    protected bool stunned;
    protected bool immobilized;
    protected float moveDir;
    protected float lastMoveDir;
    protected float watchDir;
    protected float previousWatchDir = 1f;
    protected float animatorSpeedMultiplier = 1f;
    protected float baseMovementAnimationSpeed;

    public event EventHandler OnFootStepTriggered;

    protected virtual void Awake() {
        creature = GetComponentInParent<Creature>();
        creatureAI = GetComponentInParent<CreatureAI>();
        mobMovement = GetComponentInParent<MobMovement>();
        creatureAttack = GetComponentInParent<CreatureAttack>();
        animator = GetComponent<Animator>();

        creatureAttack.OnMobAttack += MobAttack_OnMobAttack;
        creature.OnMobDied += Creature_OnMobDied;
        creature.OnCreatureStunStarted += Creature_OnCreatureStunStarted;
        creature.OnCreatureStunStopped += Creature_OnCreatureStunStopped;
        creature.OnCreatureEnabled += Creature_OnCreatureEnabled;
        mobMovement.OnMoveSpeedBuffChanged += MobMovement_OnMoveSpeedBuffChanged;
    }

    protected void RandomizeIdleAnimationStart() {
        if (animator == null) return;

        // On récupère les infos de l’état actuel
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Si l’animation actuelle est l’Idle
        if (stateInfo.IsName("Idle") || stateInfo.IsTag("Idle")) {
            // Randomize le point de départ (entre 0 et 1 = cycle complet)
            float randomTime = UnityEngine.Random.Range(0f, 1f);
            animator.Play(stateInfo.fullPathHash, 0, randomTime);
        }
    }

    private void Creature_OnCreatureEnabled(object sender, EventArgs e) {
        SpawnVisuals();
    }

    protected virtual void Start() {
        baseMovementAnimationSpeed = creature.GetCreatureSO().baseMovementAnimationSpeed;
        animatorSpeedMultiplier = baseMovementAnimationSpeed;
        animator.SetFloat("AnimationSpeedMultiplier", animatorSpeedMultiplier);

        SpawnVisuals();
    }

    protected void SpawnVisuals() {
        spawned = false;
        StartCoroutine(SetSpawnedAfterDelay(creature.GetCreatureSO().spawnAnimationDuration));

        if (creature.GetCreatureSO().hasCustomSpawnAnimation) {
            animator.SetTrigger("Spawn");
        }
    }

    protected void MobMovement_OnMoveSpeedBuffChanged(object sender, MobMovement.OnMoveSpeedBuffedEventArgs e) {
        if (!e.changeAnimatorSpeed) return;
        if (animator == null) return;
        animatorSpeedMultiplier = baseMovementAnimationSpeed * e.moveSpeedBuff;
        animator.SetFloat("AnimationSpeedMultiplier", animatorSpeedMultiplier);
    }

    protected void Update() {
        if (creature.GetDead()) return;
        if (!spawned) return;

        moveDir = mobMovement.GetMoveDirFloat();
        lastMoveDir = mobMovement.GetLastMoveDirFloat();

        HandleXScale();
        HandleAnimatorMovementBool();

    }

    protected void HandleAnimatorMovementBool() {
        if (creature.GetCreatureSO().flying) {
            moving = true;
            return;
        };

        if (moveDir != 0 && !stunned && !immobilized) {

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

    protected virtual void HandleXScale() {
        if (stunned || immobilized) return;

        if (creatureAttack.GetAttacking()) {
            if (creatureAttack.GetCurrentCreatureAttackSO().blockSwitchXScaleWhileAttacking) return;
            HandleScaleChange(creatureAttack.GetAttackDir().x);
            return;
        }

        if (moving && moveDir != 0) {
            HandleScaleChange(moveDir);
            return;
        }

        HandleScaleChange(lastMoveDir);
    }

    private void Creature_OnCreatureStunStopped(object sender, EventArgs e) {
        stunned = false;
    }

    private void Creature_OnCreatureStunStarted(object sender, EventArgs e) {
        stunned = true;
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

    private IEnumerator SetSpawnedAfterDelay(float delay) {
        float randomDir = UnityEngine.Random.Range(-1f, 1f);
        HandleScaleChange(randomDir);
        yield return new WaitForSeconds(delay);
        spawned = true;
        RandomizeIdleAnimationStart();
    }

    protected virtual void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        if(creatureAttack.GetIsPrimaryAttack()) {
            animator.SetTrigger("Attack");
        }

        if (creatureAttack.GetIsSecondaryAttack()) {
            animator.SetTrigger("Attack_Secondary");
        }
    }

    protected virtual void Creature_OnMobDied(object sender, System.EventArgs e) {
        animator.SetTrigger("Die");
        float playerDir = Player.Instance.transform.position.x - transform.position.x;
        HandleScaleChange(playerDir);

        moving = false;
        stunned = false;
        immobilized = false;
    }

    public void FootStepEvent() {
        OnFootStepTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void SetReadyToMove() {
        mobMovement.SetReadyToMoveAnimator(true);
    }

    public void SetUnReadyToMove() {
        mobMovement.SetReadyToMoveAnimator(false);
    }

    public void SetUnReadyToMoveAnimatorAndStopMoving() {
        mobMovement.SetUnReadyToMoveAnimatorAndStopMoving();
    }
}
