using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class WorkerAnimatorManager : MonoBehaviour
{
    [SerializeField] private AnimatorController joblessAnimator;
    [SerializeField] private AnimatorController hunterAnimator;
    [SerializeField] private AnimatorController guardAnimator;
    [SerializeField] private AnimatorController minerAnimator;

    private Worker worker;
    private MobAttack mobAttack;
    private MobMovement mobMovement;
    private Animator animator;

    private bool moving;
    private float moveDir;
    private float previousWatchDir = 1f;

    private void Awake() {
        worker = GetComponentInParent<Worker>();
        mobMovement = GetComponentInParent<MobMovement>();
        mobAttack = GetComponentInParent<MobAttack>();
        animator = GetComponent<Animator>();

        worker.OnJobChanged += Worker_OnJobChanged;
        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
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
        if(moving) {

            if (moveDir < 0 && previousWatchDir > 0) {
                previousWatchDir = moveDir;
                Vector3 newScale = new Vector3(-1, 1, 1);
                transform.localScale = newScale;
            }

            if (moveDir > 0 && previousWatchDir < 0) {
                previousWatchDir = moveDir;
                Vector3 newScale = new Vector3(1, 1, 1);
                transform.localScale = newScale;
            }

        } 
        
        if(mobAttack.GetAttacking()) {
            if (mobAttack.GetAttackDir().x < 0 && previousWatchDir > 0) {
                previousWatchDir = moveDir;
                Vector3 newScale = new Vector3(-1, 1, 1);
                transform.localScale = newScale;
            }

            if (mobAttack.GetAttackDir().x > 0 && previousWatchDir < 0) {
                previousWatchDir = moveDir;
                Vector3 newScale = new Vector3(1, 1, 1);
                transform.localScale = newScale;
            }
        }
    }

    private void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.SetTrigger("Attack");
    }

    private void Worker_OnJobChanged(object sender, System.EventArgs e) {
        RefreshJobAnimator();
    }

    private void RefreshJobAnimator() {

        if (worker.GetJob() == Worker.JobTypes.wild) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = .75f;
        }

        if (worker.GetJob() == Worker.JobTypes.jobless) {
            animator.runtimeAnimatorController = joblessAnimator;
            animator.speed = 1f;
        }

        if (worker.GetJob() == Worker.JobTypes.hunter) {
            animator.runtimeAnimatorController = hunterAnimator;
        }

        if (worker.GetJob() == Worker.JobTypes.guard) {
            animator.runtimeAnimatorController = guardAnimator;
        }

        if (worker.GetJob() == Worker.JobTypes.miner) {
            animator.runtimeAnimatorController = minerAnimator;
        }

        moving = false;
    }
}
