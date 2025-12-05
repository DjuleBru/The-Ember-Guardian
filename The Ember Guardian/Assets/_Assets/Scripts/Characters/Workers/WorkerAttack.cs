using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerAttack : MobAttack
{

    private WorkerAI workerAI;

    private float hunterAttackAnimationDelay = .6f;

    private float guardAttackAnimationDelay = .3f;
    private float guardTotalAttackAnimationTime = .4f;

    private float minerAttackAnimationDelay = .15f;
    private float minerTotalAttackAnimationTime = .5f;
    private float hunterAnimalAttackPointRandomizer;

    private float initialProbabilityToHaveHomingProjectileOnCreature;
    private float probabilityToHaveHomingProjectileOnCreature = .6f;
    private bool homingArrows;


    protected override void Awake() {
        base.Awake();
        workerAI = GetComponent<WorkerAI>();
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void Start() {
        WorkerStats.Instance.OnAttackParameterChanged += WorkerStats_OnAttackParameterChanged;
    }

    private void WorkerStats_OnAttackParameterChanged(object sender, EventArgs e) {
        RefreshAttackParameters();
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        RefreshAttackParameters();
    }

    private void RefreshAttackParameters() {
        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            isProjectileAttack = true;

            attackDamage = WorkerStats.Instance.GetHunterDamage();
            initialAttackDamage = WorkerStats.Instance.GetHunterDamage();
            attackCooldown = WorkerStats.Instance.GetHunterAttackCooldown();
            attackAnimationDelay = hunterAttackAnimationDelay;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            isProjectileAttack = false;

            attackDamage = WorkerStats.Instance.GetGuardDamage();
            initialAttackDamage = WorkerStats.Instance.GetGuardDamage();
            attackCooldown = WorkerStats.Instance.GetGuardAttackCooldown();
            attackAnimationDelay = guardAttackAnimationDelay;
            totalAttackAnimationTime = guardTotalAttackAnimationTime;
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            isProjectileAttack = false;

            attackDamage = WorkerStats.Instance.GetMinerDamage();
            initialAttackDamage = WorkerStats.Instance.GetMinerDamage();
            attackCooldown = WorkerStats.Instance.GetMinerAttackCooldown();
            attackAnimationDelay = minerAttackAnimationDelay;
            totalAttackAnimationTime = minerTotalAttackAnimationTime;
        }
    }

    public override void Attack() {
        if (attackTargetIDamageable is Creature) {

            float hunterAccuracyBuff = WorkerStats.Instance.GetHunterAccuracyBuff();
            probabilityToHaveHomingProjectileOnCreature *= (1 + hunterAccuracyBuff / 200);
            if (probabilityToHaveHomingProjectileOnCreature > .9f) {
                probabilityToHaveHomingProjectileOnCreature = .9f;
            }

            if(homingArrows) {
                probabilityToHaveHomingProjectileOnCreature = 1f;
            }

            homingProjectile = UnityEngine.Random.value < probabilityToHaveHomingProjectileOnCreature;
        }
        else {
            homingProjectile = false;
        }
        
        base.Attack();
    }

    protected override Vector3 GetEndPointRandomOffstetValue() {

        hunterAnimalAttackPointRandomizer = WorkerStats.Instance.GetHunterAccuracy();
        float randomized = UnityEngine.Random.Range(-hunterAnimalAttackPointRandomizer, hunterAnimalAttackPointRandomizer);

        if (homingProjectile) {
            randomized = 0;
        }

        return new Vector3(randomized, 0,0);
    }

    public void SetHomingArrows() {
        homingArrows = true;
    }

    public void ResetHomingArrowsProbability() {
        homingArrows = false;

    }

}
