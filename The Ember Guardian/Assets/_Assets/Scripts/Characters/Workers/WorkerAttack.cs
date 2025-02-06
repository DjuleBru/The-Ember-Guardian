using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerAttack : MobAttack
{

    private WorkerAI workerAI;

    private int initialHunterDamage = 10;
    private float initialHunterAttackCooldown = 3f;
    private float hunterAttackAnimationDelay = .6f;


    private int initialGuardDamage = 5;
    private float initialGuardAttackCooldown = 1f;
    private float guardAttackAnimationDelay = .3f;
    private float guardTotalAttackAnimationTime = .4f;

    private int initialMinerDamage = 1;
    private float initialMinerAttackCooldown = 1.2f;
    private float minerAttackAnimationDelay = .15f;
    private float minerTotalAttackAnimationTime = .5f;

    private void Start() {
        workerAI = GetComponent<WorkerAI>();
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            isProjectileAttack = true;

            attackDamage = initialHunterDamage;
            attackCooldown = initialHunterAttackCooldown;
            attackAnimationDelay = hunterAttackAnimationDelay;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            isProjectileAttack = false;

            attackDamage = initialGuardDamage;
            attackCooldown = initialGuardAttackCooldown;
            attackAnimationDelay = guardAttackAnimationDelay;
            totalAttackAnimationTime = guardTotalAttackAnimationTime;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            isProjectileAttack = false;

            attackDamage = initialMinerDamage;
            attackCooldown = initialMinerAttackCooldown;
            attackAnimationDelay = minerAttackAnimationDelay;
            totalAttackAnimationTime = minerTotalAttackAnimationTime;
        }
    }
}
