using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerMovement : MobMovement
{

    private WorkerAI workerAI;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    protected bool followingPlayer;

    protected override void Awake() {
        base.Awake();
        workerAI = GetComponent<WorkerAI>();
        hunterJob = GetComponent<HunterJob>();
        guardJob = GetComponent<GuardJob>();    
    }

    protected override void Start() {
        base.Start();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;
    }

    protected void Update() {
        if (!followingPlayer) return;

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            if (hunterJob.GetState() == HunterJob.HunterState.followPlayerAttackCreature) return;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            if (guardJob.GetState() == GuardJob.GuardState.followPlayerAttackCreature) return;
        }

        float minSpeed = 2f;  // Vitesse minimale
        float maxSpeed = PlayerMovement.Instance.GetTargetMoveSpeed()*1.25f; // Vitesse max = celle du joueur

        float distance = Vector3.Distance(transform.position, targetDestination);
        float speedFactor = Mathf.Clamp01(distance / 5f); // Distance max considérée pour le scaling

        float newSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedFactor);
        SetMoveSpeed(newSpeed);
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, System.EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
    }

    private IEnumerator SetTargetMoveSpeedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        float moveSpeedRandomizer = UnityEngine.Random.Range(PlayerMovement.Instance.GetTargetMoveSpeed()/15, PlayerMovement.Instance.GetTargetMoveSpeed()/10);
        SetMoveSpeed(PlayerMovement.Instance.GetTargetMoveSpeed() - moveSpeedRandomizer);

    }

    public override void SetMoveSpeed(float moveSpeed) {

        float moveSpeedBuff = 0f;
        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            moveSpeedBuff = WorkerStats.Instance.GetHunterMoveSpeedBuff();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            moveSpeedBuff = WorkerStats.Instance.GetGuardMoveSpeedBuff();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            moveSpeedBuff = WorkerStats.Instance.GetMinerMoveSpeedBuff();
        }

        this.moveSpeed = moveSpeed * (1+moveSpeedBuff);
    }
}
