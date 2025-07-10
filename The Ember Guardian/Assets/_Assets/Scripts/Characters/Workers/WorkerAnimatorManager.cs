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
    [SerializeField] private RuntimeAnimatorController engineerAnimator;
    [SerializeField] private Animator workerBodyAnimator;

    private Worker worker;
    private WorkerAI workerAI;
    private MobAttack mobAttack;
    private MobMovement mobMovement;
    private Animator animator;
    private EngineerJob engineerJob;

    private bool moving;
    private bool wildJobTypeSet;
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
        engineerJob = GetComponentInParent<EngineerJob>();

        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
        worker.OnWildJobTypeSet += Worker_OnWildJobTypeSet;
        worker.OnMobDied += Worker_OnMobDied;
        worker.OnMobDamageTaken += Worker_OnMobDamageTaken;
        engineerJob.OnOrbExtractorTriggeredDrill += EngineerJob_OnOrbExtractorTriggeredDrill;
        engineerJob.OnEngineerTurnsWrench += EngineerJob_OnEngineerTurnsWrench;
    }

    private void Worker_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        workerBodyAnimator.SetTrigger("Hit");
    }

    private void Start() {
        if (worker.GetWildJobType() != workerAI.GetJob()) return;
        RefreshJobAnimator(workerAI.GetJob());
    }

    private void Update() {
        moveDir = mobMovement.GetMoveDirFloat();

        HandleXScale();
        HandleAnimatorMovementBool();

    }

    private void Worker_OnWildJobTypeSet(object sender, EventArgs e) {
        RefreshJobAnimator(worker.GetWildJobType());
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

    private void EngineerJob_OnOrbExtractorTriggeredDrill(object sender, EventArgs e) {
        animator.SetTrigger("WorkInOrbExtractor");

        HandleScaleChange(transform.position.x);
    }
    private void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.SetTrigger("Attack");
    }

    private void EngineerJob_OnEngineerTurnsWrench(object sender, EventArgs e) {
        animator.SetTrigger("Attack");
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        //Debug.Log(workerAI.GetJob());
        if(workerAI.GetJob() == WorkerAI.JobTypes.wild) {
            RefreshJobAnimator(worker.GetWildJobType());
        } else {
            RefreshJobAnimator(workerAI.GetJob());
        }

    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        animator.SetTrigger("Die");
    }

    private void RefreshJobAnimator(WorkerAI.JobTypes jobType) {
        if (jobType == WorkerAI.JobTypes.wild) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = .75f;
        }

        if (jobType == WorkerAI.JobTypes.jobless) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = 1f;
        }

        if (jobType == WorkerAI.JobTypes.hunter) {
            animator.runtimeAnimatorController = hunterAnimator;
        }

        if (jobType == WorkerAI.JobTypes.guard) {
            animator.runtimeAnimatorController = guardAnimator;
        }

        if (jobType == WorkerAI.JobTypes.miner) {
            animator.runtimeAnimatorController = minerAnimator;
        }

        if (jobType == WorkerAI.JobTypes.engineer) {
            animator.runtimeAnimatorController = engineerAnimator;
        }
        moving = false;
    }

    public void SetReadyToMoveAnimator() {
        mobMovement.SetReadyToMoveAnimator(true);
    }
    public void SetUnreadyToMoveAnimator() {
        mobMovement.SetReadyToMoveAnimator(false);
    }
}
