using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerAnimatorManager : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController joblessAnimator;
    [SerializeField] private RuntimeAnimatorController hunterAnimator;
    [SerializeField] private RuntimeAnimatorController guardAnimator;
    [SerializeField] private RuntimeAnimatorController minerAnimator;
    [SerializeField] private Animator workerBodyAnimator;

    private Worker worker;
    private WorkerAI workerAI;
    private MobAttack mobAttack;
    private MobMovement mobMovement;
    private Animator animator;

    private bool moving;
    private float moveDir;
    private float watchDir;
    private float previousWatchDir = 1f;

    public event EventHandler OnFootstepTriggered;
    public event EventHandler OnXScaleChanged;

    private void Awake() {
        worker = GetComponentInParent<Worker>();
        workerAI = GetComponentInParent<WorkerAI>();
        mobMovement = GetComponentInParent<MobMovement>();
        mobAttack = GetComponentInParent<MobAttack>();
        animator = GetComponent<Animator>();

        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
        worker.OnMobDied += Worker_OnMobDied;
        worker.OnMobDamageTaken += Worker_OnMobDamageTaken;
    }

    private void Worker_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        workerBodyAnimator.SetTrigger("Hit");
    }

    private void Start() {
        RefreshJobAnimator();
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

        if(mobAttack.GetAttacking()) {
            HandleScaleChange(mobAttack.GetAttackDir().x);
            return;
        }

        if(worker.GetPlayerIsClose()) {
            float watchDir = Player.Instance.transform.position.x - transform.position.x;
            HandleScaleChange(watchDir);
            return;
        }


        HandleScaleChange(watchDir);
    }

    private void HandleScaleChange(float watchDir) {
        if (watchDir < 0 && previousWatchDir > 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(-1, 1, 1);
            transform.localScale = newScale;

            OnXScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        if (watchDir > 0 && previousWatchDir < 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            transform.localScale = newScale;

            OnXScaleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void TriggerFootStep() {
        OnFootstepTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void SetWatchDir(float watchDir) {
        this.watchDir = watchDir;
    }

    public float GetWatchDir() {
        return watchDir;
    }
    public float GetPreviousWatchDir() {
        return previousWatchDir;
    }

    private void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.SetTrigger("Attack");
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        RefreshJobAnimator();
    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        animator.SetTrigger("Die");
    }

    private void RefreshJobAnimator() {

        if (workerAI.GetJob() == WorkerAI.JobTypes.wild) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = .75f;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.jobless) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = 1f;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            animator.runtimeAnimatorController = hunterAnimator;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            animator.runtimeAnimatorController = guardAnimator;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            animator.runtimeAnimatorController = minerAnimator;
        }

        moving = false;
    }

}
