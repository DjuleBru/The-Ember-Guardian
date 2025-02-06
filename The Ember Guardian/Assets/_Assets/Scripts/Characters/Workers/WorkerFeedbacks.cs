using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerFeedbacks : MonoBehaviour
{
    private Worker worker;
    private WorkerAI workerAI;
    private WorkerAttack workerAttack;

    [SerializeField] private WorkerAnimatorManager animatorManager;

    [SerializeField] private ParticleSystem minerHitPS;
    [SerializeField] private Transform minerHitPSLeftPosition;
    [SerializeField] private Transform minerHitPSRightPosition;

    private void Awake() {
        worker = GetComponentInParent<Worker>();
        workerAI = GetComponentInParent<WorkerAI>();
        workerAttack = GetComponentInParent<WorkerAttack>();
    }

    private void Start() {
        workerAttack.OnMobAttackHit += WorkerAttack_OnMobAttackHit;
        animatorManager.OnXScaleChanged += AnimatorManager_OnXScaleChanged;
    }

    private void AnimatorManager_OnXScaleChanged(object sender, System.EventArgs e) {
        if (animatorManager.GetPreviousWatchDir() < 0) {
            minerHitPS.transform.position = minerHitPSRightPosition.position;
        }
        else {
            minerHitPS.transform.position = minerHitPSLeftPosition.position;
        }
    }


    private void WorkerAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        if(workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            minerHitPS.Play();
        }
    }
}
